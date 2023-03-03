using Aki.SinglePlayer.Models.Healing;
using Newtonsoft.Json;
using EFT;

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

		public SaveProfileRequest()
		{
			Exit = "left";
		}
	}
}
