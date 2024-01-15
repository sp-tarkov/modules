using EFT;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Aki.SinglePlayer.Utils.TraderServices
{
    public class TraderServiceModel
    {
        [JsonProperty("serviceType")]
        public ETraderServiceType ServiceType { get; set; }

        [JsonProperty("itemsToPay")]
        public Dictionary<string, int> ItemsToPay { get; set; }

        [JsonProperty("subServices")]
        public Dictionary<string, int> SubServices { get; set; }
    }
}