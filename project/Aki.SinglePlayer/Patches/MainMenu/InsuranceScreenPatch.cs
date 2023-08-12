using Aki.Reflection.Patching;
using EFT;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    /// <summary>
    /// Force ERaidMode to online to make interface show insurance page
    /// </summary>
    class InsuranceScreenPatch : ModulePatch
    {
        static InsuranceScreenPatch()
        {
            _ = nameof(MainMenuController.InventoryController);
        }

        protected override MethodBase GetTargetMethod()
        {
            //[CompilerGenerated]
            //private void method_67()
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

            var desiredType = typeof(MainMenuController);
            var desiredMethod = desiredType.GetMethod("method_67", BindingFlags.NonPublic | BindingFlags.Instance);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        private static void PrefixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Online;
        }

        [PatchPostfix]
        private static void PostfixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Local;
        }
    }
}
