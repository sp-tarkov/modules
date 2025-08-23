using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPT.Custom.Models;

[Serializable]
public struct DifficultyInfo
{
    public JArray this[string key]
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
                    throw new ArgumentException($"Difficulty '{key}' does not exist in DifficultyInfo.");
            }
        }
    }

    [JsonProperty("easy")]
    public JArray easy;

    [JsonProperty("hard")]
    public JArray hard;

    [JsonProperty("impossible")]
    public JArray impossible;

    [JsonProperty("normal")]
    public JArray normal;
}
