using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using BepInEx.Logging;
using EFT;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Core;

namespace SPT.Custom.Utils;

public static class DifficultyManager
{
    internal static ManualLogSource _logger = new("DifficultyManager");

    public static Dictionary<string, Dictionary<string, JObject>> Difficulties { get; private set; } = [];

    public static void Update()
    {
        // remove existing list
        Difficulties.Clear();

        // get new difficulties
        var json = RequestHandler.GetJson("/singleplayer/settings/bot/difficulties");
        Difficulties = Json.Deserialize<Dictionary<string, Dictionary<string, JObject>>>(json);
    }

    public static BotSettingsComponents Get(BotDifficulty botDifficulty, WildSpawnType role)
    {
        try
        {
            var difficultyMatrix = Difficulties[role.ToString().ToLower()];
            return Json.Deserialize<BotSettingsComponents>(difficultyMatrix[botDifficulty.ToString().ToLower()]);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not deserialize {role} ({botDifficulty}): {ex}");

            return null;
        }
    }
}
