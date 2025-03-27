using System;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.Bots;
using HarmonyLib;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Fix bosses not spawning with any followers because BSG subtract the escort amount from itself
    /// Also fix 'enable bosses' checkbox not working
    /// </summary>
    public class FixBossesHavingNoFollowersOnMediumAiAmount: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.smethod_8));
        }

        [PatchPrefix]
        public static void PatchPrefix(LocalGame __instance, WavesSettings wavesSettings, ref BossLocationSpawn[] bossLocationSpawn)
        {
            // Player has disabled bosses, filter to only PMCs
            if (!wavesSettings.IsBosses)
            {
                bossLocationSpawn =
                    Array.FindAll(bossLocationSpawn, x => x.BossType is WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC);

                return;
            }

            // Not set to medium = nothing to fix, skip
            if (wavesSettings.BotAmount != EBotAmount.Medium)
            {
                return;
            }

            // Adjust boss escort amount values
            foreach (var locationSpawn in bossLocationSpawn)
            {
                // Only adjust bosses with single escort amount value, skip others
                // e.g. skip "1,3,5,2"
                if (locationSpawn.BossEscortAmount.Length != 1)
                {
                    continue;
                }

                // Only add new value when existing value is > 0
                var existingAmount = int.Parse(locationSpawn.BossEscortAmount);
                if (existingAmount == 0)
                {
                    continue;
                }

                // Add second value to property, when client does (max - min / 2), it won't be 0
                // e.g. Reshala has value of "4", we add value "(existing)*3 (=12)", which means the client does "(12 - 4) / 2" giving us 4 escorts - same as high/as online AI amount
                locationSpawn.BossEscortAmount += $",{existingAmount * 3}";
            }
        }
    }
}
