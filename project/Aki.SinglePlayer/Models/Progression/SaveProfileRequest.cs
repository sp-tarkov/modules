using Newtonsoft.Json;
using EFT;
using System.Collections.Generic;
using Aki.SinglePlayer.Models.Healing;
using Aki.SinglePlayer.Models.RaidFix;

namespace Aki.SinglePlayer.Models.Progression
{
    public class SaveProfileRequest
	{
		[JsonProperty("exit")]
		public string Exit;

		[JsonProperty("profile")]
		public Profile Profile;

		[JsonProperty("isPlayerScav")]
		public bool IsPlayerScav;

		[JsonProperty("health")]
		public PlayerHealth Health;

		[JsonProperty("Insurance")]
		public List<AkiInsuredItemClass> Insurance;

		public SaveProfileRequest()
		{
			Exit = "left";
		}
	}
}
