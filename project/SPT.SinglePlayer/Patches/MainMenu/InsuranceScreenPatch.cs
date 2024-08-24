using SPT.Reflection.Patching;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Force ERaidMode to online to make interface show insurance page
    /// </summary>
    public class InsuranceScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            //[CompilerGenerated]
            //private void method_XX()
            //{
            //    if (this.raidSettings_0.SelectedLocation.Id == "laboratory")
            //    {
            //        this.raidSettings_0.WavesSettings.IsBosses = true;
            //    }
            //    if (this.raidSettings_0.RaidMode == ERaidMode.Online)
            //    {
            //        this.method_40();
            //        return;
            //    }
            //    this.method_41();
            //}

            return AccessTools.Method(typeof(MainMenuController), nameof(MainMenuController.method_76));
        }

        [PatchPrefix]
        public static void PrefixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Online;
        }
    }
}
