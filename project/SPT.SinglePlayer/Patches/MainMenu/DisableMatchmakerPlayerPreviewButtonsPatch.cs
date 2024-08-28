using System.Reflection;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.SinglePlayer.Patches.MainMenu
{
    public class DisableMatchmakerPlayerPreviewButtonsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MatchMakerPlayerPreview), nameof(MatchMakerPlayerPreview.Show));
        }

        [PatchPrefix]
        public static void PatchPrefix(ref GClass3114 contextInteractions)
        {
            // clear with a null to stop "looking for group/create group" buttons
            // they handle nulls so don't worry
            contextInteractions = null;
        }
    }
}