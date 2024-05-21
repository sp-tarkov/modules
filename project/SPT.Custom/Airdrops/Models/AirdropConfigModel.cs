using Newtonsoft.Json;
using System.Collections.Generic;

namespace SPT.Custom.Airdrops.Models
{
    public class AirdropConfigModel
    {
        [JsonProperty("airdropChancePercent")]
        public AirdropChancePercent AirdropChancePercent { get; set; }
        
        [JsonProperty("airdropMinStartTimeSeconds")]
        public int AirdropMinStartTimeSeconds { get; set; }
        
        [JsonProperty("airdropMaxStartTimeSeconds")]
        public int AirdropMaxStartTimeSeconds { get; set; }
        
        [JsonProperty("planeMinFlyHeight")]
        public int PlaneMinFlyHeight { get; set; }
        
        [JsonProperty("planeMaxFlyHeight")]
        public int PlaneMaxFlyHeight { get; set; }
        
        [JsonProperty("planeVolume")]
        public float PlaneVolume { get; set; }
        
        [JsonProperty("planeSpeed")]
        public float PlaneSpeed { get; set; }
        
        [JsonProperty("crateFallSpeed")]
        public float CrateFallSpeed { get; set; }

        [JsonProperty("containerIds")]
        public Dictionary<string, string> ContainerIds { get; set; }
    }

    public class AirdropChancePercent
    {
        [JsonProperty("bigmap")]
        public int Bigmap { get; set; }
        
        [JsonProperty("woods")]
        public int Woods { get; set; }
        
        [JsonProperty("lighthouse")]
        public int Lighthouse { get; set; }
        
        [JsonProperty("shoreline")]
        public int Shoreline { get; set; }
        
        [JsonProperty("interchange")]
        public int Interchange { get; set; }
        
        [JsonProperty("reserve")]
        public int Reserve { get; set; }
        
        [JsonProperty("tarkovStreets")]
        public int TarkovStreets { get; set; }
    }
}