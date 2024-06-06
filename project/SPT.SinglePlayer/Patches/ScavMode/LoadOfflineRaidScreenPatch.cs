using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using EFT;
using EFT.Bots;
using EFT.UI.Matchmaker;
using EFT.UI.Screens;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using OfflineRaidAction = System.Action;

// DON'T FORGET TO UPDATE REFERENCES IN CONSTRUCTOR
// AND IN THE LoadOfflineRaidScreenForScavs METHOD AS WELL

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class LoadOfflineRaidScreenPatch : ModulePatch
    {
        private static readonly MethodInfo _onReadyScreenMethod;
        private static readonly FieldInfo _isLocalField;
        private static readonly FieldInfo _menuControllerField;

        static LoadOfflineRaidScreenPatch()
        {
            _ = nameof(MainMenuController.InventoryController);
            _ = nameof(TimeAndWeatherSettings.IsRandomWeather);
            _ = nameof(BotControllerSettings.IsScavWars);
            _ = nameof(WavesSettings.IsBosses);
            _ = MatchmakerPlayerControllerClass.MAX_SCAV_COUNT; // UPDATE REFS TO THIS CLASS BELOW !!!

            // `MatchmakerInsuranceScreen` OnShowNextScreen
            _onReadyScreenMethod = AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_44));

            _isLocalField = AccessTools.Field(typeof(MainMenuController), "bool_0");
            _menuControllerField = typeof(TarkovApplication).GetFields(PatchConstants.PrivateFlags).FirstOrDefault(x => x.FieldType == typeof(MainMenuController));

            if (_menuControllerField == null)
            {
                Logger.LogError($"LoadOfflineRaidScreenPatch() menuControllerField is null and could not be found in {nameof(TarkovApplication)} class");
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            // `MatchMakerSelectionLocationScreen` OnShowNextScreen
            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_71));
        }

        [PatchTranspiler]
        private static IEnumerable<CodeInstruction> PatchTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            /* The original msil looks something like this:
             *   0	0000	ldarg.0
             *   1	0001	call        instance void MainMenuController::method_69()
             *   2	0006	ldarg.0
             *   3	0007	call        instance void MainMenuController::method_41()
             *   4	000C	ldarg.0
             *   5	000D	call        instance bool MainMenuController::method_46()
             *   6	0012	brtrue.s    8 (0015) ldarg.0 
             *   7	0014	ret
             *   8	0015	ldarg.0
             *   9	0016	ldfld       class EFT.RaidSettings MainMenuController::raidSettings_0
             *   10	001B	callvirt    instance bool EFT.RaidSettings::get_IsPmc()
             *   11	0020	brfalse.s   15 (0029) ldarg.0 
             *   12	0022	ldarg.0
             *   13	0023	call        instance void MainMenuController::method_42()
             *   14	0028	ret
             *   15	0029	ldarg.0
             *   16	002A	call        instance void MainMenuController::method_44()
             *   17	002F	ret
             *
             *   The goal is to replace the call to method_44 with our own LoadOfflineRaidScreenForScav function.
             *   method_44 expects one argument which is the implicit "this" pointer.
             *   The ldarg.0 instruction loads "this" onto the stack and the function call will consume it.
             *   But because our own LoadOfflineRaidScreenForScav method is static
             *   it won't consume a "this" pointer from the stack, so we have to remove the ldarg.0 instruction.
             *   But the brfalse instruction at 0020 jumps to the ldarg.0, so we can not simply delete it.
             *   Instead, we first need to transfer the jump label from the ldarg.0 instruction to our new
             *   call instruction and only then we remove it.
             */
            var codes = new List<CodeInstruction>(instructions);
            var onReadyScreenMethodOperand = AccessTools.Method(typeof(MainMenuController), _onReadyScreenMethod.Name);

            var callCodeIndex = codes.FindLastIndex(code => code.opcode == OpCodes.Call
                                                        && (MethodInfo)code.operand == onReadyScreenMethodOperand);

            if (callCodeIndex == -1)
            {
                throw new Exception($"{nameof(LoadOfflineRaidScreenPatch)} failed: Could not find {nameof(_onReadyScreenMethod)} reference code.");
            }

            var loadThisIndex = callCodeIndex - 1;
            if (codes[loadThisIndex].opcode != OpCodes.Ldarg_0)
            {
                throw new Exception($"{nameof(LoadOfflineRaidScreenPatch)} failed: Expected ldarg.0 before call instruction but found {codes[loadThisIndex]}");
            }

            // Overwrite the call instruction with the call to LoadOfflineRaidScreenForScav, preserving the label for the 0020 brfalse jump
            codes[callCodeIndex] = new CodeInstruction(OpCodes.Call, 
                AccessTools.Method(typeof(LoadOfflineRaidScreenPatch), nameof(LoadOfflineRaidScreenForScav))) {
                labels = codes[loadThisIndex].labels
            };

            // Remove the ldarg.0 instruction which we no longer need because LoadOfflineRaidScreenForScav is static
            codes.RemoveAt(loadThisIndex);

            return codes.AsEnumerable();
        }

        private static void LoadOfflineRaidScreenForScav()
        {
            var profile = PatchConstants.BackEndSession.Profile;
            var menuController = (object)GetMenuController();

            // Get fields from MainMenuController.cs
            var raidSettings = Traverse.Create(menuController).Field("raidSettings_0").GetValue<RaidSettings>();

            // Find the private field of type `MatchmakerPlayerControllerClass`
            var matchmakerPlayersController = menuController.GetType()
                .GetFields(AccessTools.all)
                .Single(field => field.FieldType == typeof(MatchmakerPlayerControllerClass))
                ?.GetValue(menuController) as MatchmakerPlayerControllerClass;

            var gclass = new MatchmakerOfflineRaidScreen.GClass3181(profile?.Info, ref raidSettings, matchmakerPlayersController, ESessionMode.Regular);

            gclass.OnShowNextScreen += LoadOfflineRaidNextScreen;

            // `MatchmakerOfflineRaidScreen` OnShowReadyScreen
            gclass.OnShowReadyScreen += (OfflineRaidAction)Delegate.CreateDelegate(typeof(OfflineRaidAction), menuController, nameof(MainMenuController.method_75));
            gclass.ShowScreen(EScreenState.Queued);
        }

        private static void LoadOfflineRaidNextScreen()
        {
            var menuController = GetMenuController();

            var raidSettings = Traverse.Create(menuController).Field("raidSettings_0").GetValue<RaidSettings>();
            if (raidSettings.SelectedLocation.Id == "laboratory")
            {
                raidSettings.WavesSettings.IsBosses = true;
            }

            // Set offline raid values
            _isLocalField.SetValue(menuController, raidSettings.Local);

            // Load ready screen method
            _onReadyScreenMethod.Invoke(menuController, null);
        }

        private static MainMenuController GetMenuController()
        {
            return _menuControllerField.GetValue(ClientAppUtils.GetMainApp()) as MainMenuController;
        }
    }
}