using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.Bots;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Fix bosses not spawning with any followers because BSG subtract the escort amount from itself
    /// </summary>
    public class FixBossesHavingNoFollowersOnMediumAiAmount: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.smethod_8));
        }

        [PatchPrefix]
        public static void PatchPrefix(LocalGame __instance, WavesSettings wavesSettings, ref BossLocationSpawn[] bossLocationSpawn, ref bool isPVEOffline)
        {
            // Flag as false to allow player to disable bosses in pre-raid screen
            isPVEOffline = false;

            // Not a boss and not set to medium, skip
            if (wavesSettings is not { IsBosses: true, BotAmount: EBotAmount.Medium })
            {
                return;
            }

            foreach (var locationSpawn in bossLocationSpawn)
            {
                // Only adjust bosses with single escort amount value, skip others
                if (locationSpawn.BossEscortAmount.Length != 1)
                {
                    continue;
                }

                // Only add new value when existing value is > 0
                var existingAmount = int.Parse(locationSpawn.BossEscortAmount);
                if (existingAmount == 0)
                {
                    // Don't add more data to boss with 0 followers
                    continue;
                }

                // Add second value to property, when client does (max - min / 2), it won't be 0
                // e.g. reshala has value of "4", we add value "(existing)*3 (=12)", which means the client does "(12 - 4) / 2" giving us 4 escorts - same as high/as online ai amount
                locationSpawn.BossEscortAmount += $",{existingAmount * 3}";
            }
        }
    }
}
