using Newtonsoft.Json;

namespace Aki.Custom.Airdrops.Models
{
    public class AirdropLootModel
    {
        [JsonProperty("tpl")]
        public string Tpl { get; set; }
        
        [JsonProperty("isPreset")]
        public bool IsPreset { get; set; }
        
        [JsonProperty("stackCount")]
        public int StackCount { get; set; }
        
        [JsonProperty("id")]
        public string ID { get; set; }
    }
}