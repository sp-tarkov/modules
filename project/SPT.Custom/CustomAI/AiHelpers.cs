using EFT;
using System.Collections.Generic;

namespace SPT.Custom.CustomAI
{
    public static class AiHelpers
    {
        /// <summary>
        /// Bot is a PMC when it has IsStreamerModeAvailable flagged and has a wildspawn type of 'pmcBEAR' or 'pmcUSEC'
        /// </summary>
        /// <param name="botRoleToCheck">Bots role</param>
        /// <param name="botOwner">Bot details</param>
        /// <returns></returns>
        public static bool IsPMC(this BotOwner botOwner)
        {
            return botOwner.Profile.Side != EPlayerSide.Savage;
        }

        public static bool IsSimulatedPlayerScav(this BotOwner botOwner)
        {
            return botOwner.Profile.Info.Settings.Role == WildSpawnType.assault && !string.IsNullOrEmpty(botOwner.Profile.Info.MainProfileNickname);
        }

        public static List<BotOwner> GetAllMembers(this BotsGroup group)
        {
            List<BotOwner> members = [];

            if (group == null)
            {
                return members;
            }

            for (int m = 0; m < group.MembersCount; m++)
            {
                members.Add(group.Member(m));
            }

            return members;
        }

        /// <summary>
        /// Returns true if the player is found in the collection by searching for matching player Id's
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playerToCheck"></param>
        /// <returns></returns>
        public static bool ContainsPlayer(this IEnumerable<IPlayer> players, IPlayer playerToCheck)
        {
            if (playerToCheck == null)
            {
                return false;
            }

            foreach (var player in players)
            {
                if (player != null && player.Id == playerToCheck.Id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
