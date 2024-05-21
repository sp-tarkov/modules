﻿using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

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
            ____notAvailableState.GetComponent<HoverTooltipArea>().SetMessageText("<color=#51c6db>SPT</color> is already PvE.");
            return false;
        }
    }
}
