using EFT;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    public class RaidTimeRequest
    {
        public RaidTimeRequest(ESideType side, string location)
        {
            Side = side;
            Location = location;
        }

        public ESideType Side { get; set; }
        public string Location { get; set; }
    }
}
