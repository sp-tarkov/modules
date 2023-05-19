using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
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

namespace Aki.SinglePlayer.Patches.ScavMode
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

            var menuControllerType = typeof(MainMenuController);

            _onReadyScreenMethod = menuControllerType.GetMethod("method_39", PatchConstants.PrivateFlags);
            _isLocalField = menuControllerType.GetField("bool_0", PatchConstants.PrivateFlags);
            _menuControllerField = typeof(TarkovApplication).GetFields(PatchConstants.PrivateFlags).FirstOrDefault(x => x.FieldType == typeof(MainMenuController));

            if (_menuControllerField == null)
            {
                Logger.LogError($"LoadOfflineRaidScreenPatch() menuControllerField is null and could not be found in {nameof(TarkovApplication)} class");
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(MainMenuController).GetMethod("method_63", PatchConstants.PrivateFlags);
        }

        [PatchTranspiler]
        private static IEnumerable<CodeInstruction> PatchTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // The original method call that we want to replace
            var onReadyScreenMethodIndex = -1;
            var onReadyScreenMethodCode = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MainMenuController), _onReadyScreenMethod.Name));

            // We additionally need to replace an instruction that jumps to a label on certain conditions, since we change the jump target instruction
            var jumpWhenFalse_Index = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == onReadyScreenMethodCode.opcode && codes[i].operand == onReadyScreenMethodCode.operand)
                {
                    onReadyScreenMethodIndex = i;
                    continue;
                }

                if (codes[i].opcode == OpCodes.Brfalse)
                {
                    if (jumpWhenFalse_Index != -1)
                    {
                        // If this warning is ever logged, the condition for locating the exact brfalse instruction will have to be updated
                        Logger.LogWarning($"[{nameof(LoadOfflineRaidScreenPatch)}] Found extra instructions with the brfalse opcode! " +
                                          "This breaks an old assumption that there is only one such instruction in the method body and is now very likely to cause bugs!");
                    }
                    jumpWhenFalse_Index = i;
                }
            }

            if (onReadyScreenMethodIndex == -1)
            {
                throw new Exception($"{nameof(LoadOfflineRaidScreenPatch)} failed: Could not find {nameof(_onReadyScreenMethod)} reference code.");
            }

            if (jumpWhenFalse_Index == -1)
            {
                throw new Exception($"{nameof(LoadOfflineRaidScreenPatch)} failed: Could not find jump (brfalse) reference code.");
            }

            // Define the new jump label
            var brFalseLabel = generator.DefineLabel();

            // We build the method call for our substituted method and replace the initial method call with our own, also adding our new label
            var callCode = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LoadOfflineRaidScreenPatch), nameof(LoadOfflineRaidScreenForScav))) { labels = { brFalseLabel }};
            codes[onReadyScreenMethodIndex] = callCode;

            // We build a new brfalse instruction and give it our new label, then replace the original brfalse instruction
            var newBrFalseCode = new CodeInstruction(OpCodes.Brfalse, brFalseLabel);
            codes[jumpWhenFalse_Index] = newBrFalseCode;

            // This will remove a stray ldarg.0 instruction. It's only needed if we wanted to reference something from `this` in the method body.
            // This is done last to ensure that previous instruction indexes don't shift around (probably why this used to just turn it into a Nop OpCode)
            codes.RemoveAt(onReadyScreenMethodIndex - 1);

            return codes.AsEnumerable();
        }

        private static void LoadOfflineRaidScreenForScav()
        {
            var profile = PatchConstants.BackEndSession.Profile;
            var menuController = (object)GetMenuController();
            var raidSettings = Traverse.Create(menuController).Field("raidSettings_0").GetValue<RaidSettings>();
            var matchmakerPlayersController = Traverse.Create(menuController).Field("gclass2781_0").GetValue<GClass2781>();
            var gclass = new MatchmakerOfflineRaidScreen.GClass2770(profile?.Info, ref raidSettings, matchmakerPlayersController);

            gclass.OnShowNextScreen += LoadOfflineRaidNextScreen;

            // ready method
            gclass.OnShowReadyScreen += (OfflineRaidAction)Delegate.CreateDelegate(typeof(OfflineRaidAction), menuController, "method_67");
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

            // set offline raid values
            _isLocalField.SetValue(menuController, raidSettings.Local);

            // load ready screen method
            _onReadyScreenMethod.Invoke(menuController, null);
        }

        private static MainMenuController GetMenuController()
        {
            return _menuControllerField.GetValue(ClientAppUtils.GetMainApp()) as MainMenuController;
        }
    }
}