using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Remove ability to alter the game mode
    /// </summary>
    public class DisableGameModeAdjustButtonPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ChangeGameModeButton), nameof(ChangeGameModeButton.Show));
        }

        [PatchPrefix]
        public static bool PatchPrefix(ESessionMode sessionMode, Profile profile, ref GameObject ____notAvailableState)
        {
            ____notAvailableState.SetActive(false);
            return false;
        }
    }
}
