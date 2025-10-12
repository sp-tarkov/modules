using System.Collections.Generic;
using EFT;

namespace SPT.Custom.CustomAI;

public static class AIExtensions
{
    /// <summary>
    /// Determines whether the bot is a PMC based on its <see cref="EPlayerSide"/>
    /// </summary>
    /// <param name="botOwner">Bot details to evaluate</param>
    /// <returns>
    /// <see langword="true"/> if the bot is a PMC <br/>
    /// <see langword="false"/> if the bot is not a PMC (i.e. a scav)
    /// </returns>
    public static bool IsPMC(this BotOwner botOwner)
    {
        return botOwner.Profile.Side != EPlayerSide.Savage;
    }

    /// <summary>
    /// Determines if the bot owner represents a simulated player scav
    /// </summary>
    /// <param name="botOwner">The bot owner instance to evaluate</param>
    /// <returns>
    /// <see langword="true"/> if the bot owner's role is assault and the main profile nickname is not empty <br/>
    /// <see langword="false"/> otherwise
    /// </returns>
    public static bool IsSimulatedPlayerScav(this BotOwner botOwner)
    {
        return botOwner.Profile.Info.Settings.Role == WildSpawnType.assault
            && !string.IsNullOrEmpty(botOwner.Profile.Info.MainProfileNickname);
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
