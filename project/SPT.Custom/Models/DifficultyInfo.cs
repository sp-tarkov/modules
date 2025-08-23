using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPT.Custom.Models;

[Serializable]
public class DifficultyInfo
{
    public object this[string key]
    {
        get
        {
            return key switch
            {
                "easy" => easy,
                "normal" => normal,
                "hard" => hard,
                "impossible" => impossible,
                _ => throw new ArgumentException($"Difficulty '{key}' does not exist in DifficultyInfo."),
            };
        }
    }

    [JsonProperty("easy")]
    private JObject easy { get; set; }

    [JsonProperty("normal")]
    private JObject normal { get; set; }

    [JsonProperty("hard")]
    private JObject hard { get; set; }

    [JsonProperty("impossible")]
    private JObject impossible { get; set; }
}
