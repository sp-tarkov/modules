using EFT;
using Newtonsoft.Json;

namespace SPT.SinglePlayer.Models.ScavMode;

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