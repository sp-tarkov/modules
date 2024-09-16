using EFT;
using EFT.UI;

namespace SPT.Custom.CustomAI
{
    public static class AiHelpers
    {
        /// <summary>
        /// Bot is a PMC when it has IsStreamerModeAvailable flagged and has a wildspawn type of 'pmcBEAR' or 'pmcUSEC'
        /// </summary>
        /// <param name="botRoleToCheck">Bots role</param>
        /// <param name="___botOwner_0">Bot details</param>
        /// <returns></returns>
        public static bool BotIsSptPmc(WildSpawnType botRoleToCheck, BotOwner ___botOwner_0)
        {
            if (___botOwner_0.Profile.Info.IsStreamerModeAvailable)
            {
                // PMCs can sometimes have their role changed to 'assaultGroup' by the client, we need an alternate way to figure out if they're a spt pmc
                return true;
            }

            return botRoleToCheck == WildSpawnType.pmcBEAR || botRoleToCheck == WildSpawnType.pmcUSEC;
        }

        public static bool BotIsPlayerScav(WildSpawnType role, string nickname)
        {
            if (role == WildSpawnType.assault && nickname.Contains("("))
            {
                // Check bot is pscav by looking for the opening parentheses of their nickname e.g. scavname (pmc name)
                return true;
            }

            return false;
        }

        public static bool BotIsSimulatedPlayerScav(WildSpawnType role, BotOwner botOwner)
        {
            // Assault and has "(" character in name = simulated p scav
            var nicknameContainsPScvCharacter = botOwner.Profile.Info.Nickname?.Contains("(");
            return nicknameContainsPScvCharacter.HasValue && nicknameContainsPScvCharacter.Value && role == WildSpawnType.assault;
        }
    }
}
