using System.Collections.Generic;
using EFT;
using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Custom.Models;

namespace Aki.Custom.Utils
{
    public static class DifficultyManager
    {
        public static List<DifficultyInfo> Difficulties { get; private set; }

        static DifficultyManager()
        {
            Difficulties = new List<DifficultyInfo>();
        }

        public static void Update()
        {
            // remove existing list
            Difficulties.Clear();

            // get new difficulties
            var json = RequestHandler.GetJson("/singleplayer/settings/bot/difficulties");
            Difficulties = Json.Deserialize<List<DifficultyInfo>>(json);
        }

        public static string Get(BotDifficulty botDifficulty, WildSpawnType role)
        {
            foreach (var entry in Difficulties)
            {
                if (botDifficulty.ToString().ToLower() == entry.Difficulty
                && role.ToString().ToLower() == entry.Role)
                {
                    return entry.Data;
                }
            }

            return string.Empty;
        }
    }
}
