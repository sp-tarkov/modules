using Aki.Reflection.Patching;
using System.Reflection;
using Aki.Reflection.Utils;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class RemoveUsedBotProfilePatch : ModulePatch
    {
        static RemoveUsedBotProfilePatch()
        {
            _ = nameof(IGetProfileData.ChooseProfile);
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsPresets).BaseType.GetMethods().SingleCustom(m => m.Name == nameof(BotsPresets.GetNewProfile) && m.IsVirtual);
        }
        
        /// <summary>
        /// BotsPresets.GetNewProfile()
        [PatchPrefix]
        private static bool PatchPrefix(ref bool withDelete)
        {
            withDelete = true;

            return true; // Do original method
        }
    }
}
