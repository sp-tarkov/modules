using Aki.Reflection.Patching;
using HarmonyLib;
using System;
using System.Reflection;
using EFT;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class PostRaidHealingPricePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(Profile.TraderInfo);
            var desiredMethod = desiredType.GetMethod("UpdateLevel", BindingFlags.NonPublic | BindingFlags.Instance);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        protected static void PatchPrefix(Profile.TraderInfo __instance)
        {
            if (__instance.Settings == null)
            {
                return;
            }

            var loyaltyLevel = __instance.Settings.GetLoyaltyLevel(__instance);
            var loyaltyLevelSettings = __instance.Settings.GetLoyaltyLevelSettings(loyaltyLevel);

            if (loyaltyLevelSettings == null)
            {
                throw new IndexOutOfRangeException($"Loyalty level {loyaltyLevel} not found.");
            }

            Traverse.Create(__instance).Property("CurrentLoyalty").SetValue(loyaltyLevelSettings.Value);
        }
    }
}