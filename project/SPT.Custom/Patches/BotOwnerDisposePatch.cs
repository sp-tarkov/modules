using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace SPT.Custom.Patches
{
    /**
     * BotOwner doesn't call SetOff on the CalledData object when a bot is disposed, this can result
     * in bots that are no longer alive having their `OnEnemyAdd` method called
     */
    public class BotOwnerDisposePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.Dispose));
        }

        [PatchPrefix]
        private static void PatchPrefix(BotOwner __instance)
        {
            if (__instance.CalledData != null)
            {
                __instance.CalledData.SetOff();
            }
        }
    }
}
