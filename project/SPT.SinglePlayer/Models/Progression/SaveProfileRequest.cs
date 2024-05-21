using Newtonsoft.Json;
using EFT;
using System.Collections.Generic;
using SPT.SinglePlayer.Models.Healing;
using SPT.SinglePlayer.Models.RaidFix;

namespace SPT.SinglePlayer.Models.Progression
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

		[JsonProperty("insurance")]
		public List<SPTInsuredItemClass> Insurance;

		public SaveProfileRequest()
		{
			Exit = "left";
		}
	}
}
