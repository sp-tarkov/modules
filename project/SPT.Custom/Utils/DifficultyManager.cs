using System.Collections.Generic;
using EFT;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;

namespace SPT.Custom.Utils
{
    public static class DifficultyManager
    {
        public static Dictionary<string, DifficultyInfo> Difficulties { get; private set; }

        static DifficultyManager()
        {
            Difficulties = new Dictionary<string, DifficultyInfo>();
        }

        public static void Update()
        {
            // remove existing list
            Difficulties.Clear();

            // get new difficulties
            var json = RequestHandler.GetJson("/singleplayer/settings/bot/difficulties");
            Difficulties = Json.Deserialize<Dictionary<string, DifficultyInfo>>(json);
        }

        public static string Get(BotDifficulty botDifficulty, WildSpawnType role)
        {
            var difficultyMatrix = Difficulties[role.ToString().ToLower()];
            return Json.Serialize(difficultyMatrix[botDifficulty.ToString().ToLower()]);
        }
    }
}
