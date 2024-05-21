using SPT.Common.Http;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using TMPro;
using System.Linq;

namespace SPT.Custom.Patches
{
    public class DisablePvEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ChangeGameModeButton), nameof(ChangeGameModeButton.Show));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ESessionMode sessionMode, Profile profile, ref GameObject ____notAvailableState)
        {
            ____notAvailableState.SetActive(true);
            Object.FindObjectsOfType<HoverTooltipArea>().Where(o => o.name == "Locked").SingleOrDefault()?.SetMessageText("<color=#51c6db>SPT-SPT</color> is already PvE.");

            return false;
        }
    }
}
