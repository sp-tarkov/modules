using Newtonsoft.Json;
using System.Collections.Generic;

namespace Aki.Custom.Airdrops.Models
{
    public class AirdropLootResultModel
    {
        [JsonProperty("dropType")]
        public string DropType { get; set; }

        [JsonProperty("loot")]
        public IEnumerable<AirdropLootModel> Loot { get;set;}
    }


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