using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using BepInEx.Logging;
using EFT;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Core;
using SPT.Custom.Models;

namespace SPT.Custom.Utils;

public static class DifficultyManager
{
    public static Dictionary<string, DifficultyInfo> Difficulties { get; private set; } = [];

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
        return Json.SerializeIndented(difficultyMatrix[botDifficulty.ToString().ToLower()]);
    }
}
