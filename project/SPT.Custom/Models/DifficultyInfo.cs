using System;
using Newtonsoft.Json;

namespace SPT.Custom.Models;

[Serializable]
public readonly struct DifficultyInfo
{
    public readonly object this[string key]
    {
        get
        {
            return key switch
            {
                "easy" => easy,
                "hard" => hard,
                "impossible" => impossible,
                "normal" => normal,
                _ => throw new ArgumentException($"Difficulty '{key}' does not exist in DifficultyInfo."),
            };
        }
    }

    [JsonProperty("easy")]
    private BotSettingsComponents easy { get; }

    [JsonProperty("hard")]
    private BotSettingsComponents hard { get; }

    [JsonProperty("impossible")]
    private BotSettingsComponents impossible { get; }

    [JsonProperty("normal")]
    private BotSettingsComponents normal { get; }
}
