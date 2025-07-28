using System.Reflection;
using System.Threading.Tasks;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Custom.Utils;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace SPT.Custom.Patches
{
    public class BetaLogoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(TarkovApplication), IsTargetMethod);
        }

        private bool IsTargetMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            return method.ReturnType == typeof(Task)
                && parameters.Length == 4
                && parameters[0].ParameterType == typeof(Profile)
                && parameters[1].ParameterType == typeof(ProfileStatusClass);
        }

        [PatchPrefix]
        public static void PatchPrefix(Profile profile)
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
        public static void PatchPostfix(ref TextMeshProUGUI ____label, Profile ___profile_0)
        {
            ____label.text = $"{MenuNotificationManager.CommitHash}";
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
        public static bool PatchPrefix(
            int screenHeight,
            int screenWidth,
            int rectHeight,
            int rectWidth,
            ref Vector2 __result
        )
        {
            System.Random random = new System.Random();

            int maxX = (screenWidth / 4) - (rectWidth / 2);
            int maxY = (screenHeight / 4) - (rectHeight / 2);
            int newX = random.Next(-maxX, maxX);
            int newY = random.Next(-maxY, maxY);

            __result = new Vector2(newX, newY);

            return false; // Skip original
        }
    }
}
