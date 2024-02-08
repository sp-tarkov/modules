using EFT.InventoryLogic.BackendInventoryInteraction;
using Newtonsoft.Json;

namespace Aki.SinglePlayer.Models.ScavMode
{
    public class SellAllRequest
    {
        [JsonProperty("Action")]
        public string Action;

        [JsonProperty("totalValue")]
        public int TotalValue;

        [JsonProperty("fromOwner")]
        public OwnerInfo FromOwner;

        [JsonProperty("toOwner")]
        public OwnerInfo ToOwner;
    }
}
