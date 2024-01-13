using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Fixes an issue with the visor toggle sound not following the player in offline raids
    /// </summary>
    public class PlayerToggleSoundFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.PlayToggleSound));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool previousState, bool isOn, AudioClip toggleOn, AudioClip toggleOff, Player __instance)
        {
            // Don't change anything and execute original method if it's not the player that triggers the method
            if (!__instance.IsYourPlayer)
            {
                return true;
            }

            if (previousState != isOn)
            {
                Singleton<BetterAudio>.Instance.PlayNonspatial(isOn ? toggleOn : toggleOff, BetterAudio.AudioSourceGroupType.Character);
            }

            previousState = isOn;
            return false;
        }
    }
}