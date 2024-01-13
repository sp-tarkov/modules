using EFT;
using Comfort.Common;
using System.Reflection;
using Aki.Reflection.Patching;
using System.Collections;
using EFT.HealthSystem;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class TinnitusFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BetterAudio), nameof(BetterAudio.StartTinnitusEffect));
        }

        // checks on invoke whether the player is stunned before allowing tinnitus
        [PatchPrefix]
        static bool PatchPrefix()
        {
            var baseMethod = AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.FindActiveEffect));

            bool shouldInvoke = baseMethod
                .MakeGenericMethod(typeof(ActiveHealthController)
                .GetNestedType("Stun", BindingFlags.Instance | BindingFlags.NonPublic))
                .Invoke(Singleton<GameWorld>.Instance.MainPlayer.ActiveHealthController, new object[] { EBodyPart.Common }) != null;

            return shouldInvoke;
        }

        // prevent null coroutine exceptions
        static IEnumerator CoroutinePassthrough()
        {
            yield return null;
            yield break;
        }
    }
}