using Aki.Reflection.Patching;
using EFT.UI;
using EFT;
using HarmonyLib;
using System.Reflection;
using Aki.SinglePlayer.Utils.MainMenu;
using TMPro;
using UnityEngine;

namespace Aki.SinglePlayer.Patches.MainMenu
{
    public class BetaLogoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.method_28));
        }

        [PatchPrefix]
        private static void PatchPrefix(Profile profile)
        {
            MonoBehaviourSingleton<PreloaderUI>.Instance.SetWatermarkStatus(profile, true);
        }
    }

    public class BetaLogoPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ClientWatermark), nameof(ClientWatermark.method_0));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref TextMeshProUGUI ____label, Profile ___profile_0)
        {
            ____label.text = $"{MenuNotificationManager.commitHash}";
        }
    }

    public class BetaLogoPatch3 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ClientWatermark), nameof(ClientWatermark.smethod_0));
        }

        // Prefix so the logic isn't being duplicated.
        [PatchPrefix]
        private static bool PatchPrefix(int screenHeight, int screenWidth, int rectHeight, int rectWidth, ref Vector2 __result)
        {
            System.Random random = new System.Random();

            int maxX = (screenWidth / 4) - (rectWidth / 2);
            int maxY = (screenHeight / 4) - (rectHeight / 2);
            int newX = random.Next(-maxX, maxX);
            int newY = random.Next(-maxY, maxY);

            __result = new Vector2(newX, newY);

            // Skip original
            return false;
        }
    }
}
