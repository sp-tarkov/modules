using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Aki.Custom.Models
{
    [Serializable]
    public struct DifficultyInfo
    {
        [JsonProperty("easy")]
        public Dictionary<string, object> easy;

        [JsonProperty("hard")]
        public Dictionary<string, object> hard;

        [JsonProperty("impossible")]
        public Dictionary<string, object> impossible;

        [JsonProperty("normal")]
        public Dictionary<string, object> normal;

        public Dictionary<string, object> GetDifficultyString(string difficulty)
        {
            // Find the field using reflection
            FieldInfo fieldInfo = typeof(DifficultyInfo).GetField(difficulty, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo == null)
            {
                throw new ArgumentException($"Difficulty '{difficulty}' does not exist in DifficultyInfo.");
            }

            return (Dictionary<string, object>) fieldInfo.GetValue(this);
        }
    }
}