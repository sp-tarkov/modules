using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPT.Custom.Models;

[Serializable]
public struct DifficultyInfo
{
    public Dictionary<string, object> this[string key]
    {
        get
        {
            switch (key)
            {
                case "easy":
                    return easy;
                case "hard":
                    return hard;
                case "impossible":
                    return impossible;
                case "normal":
                    return normal;
                default:
                    throw new ArgumentException(
                        $"Difficulty '{key}' does not exist in DifficultyInfo."
                    );
            }
        }
    }

    [JsonProperty("easy")]
    public Dictionary<string, object> easy;

    [JsonProperty("hard")]
    public Dictionary<string, object> hard;

    [JsonProperty("impossible")]
    public Dictionary<string, object> impossible;

    [JsonProperty("normal")]
    public Dictionary<string, object> normal;
}