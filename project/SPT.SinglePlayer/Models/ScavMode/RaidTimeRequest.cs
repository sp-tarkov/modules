using EFT;

namespace SPT.SinglePlayer.Models.ScavMode;

public class RaidTimeRequest(ESideType side, string location)
{
    public ESideType Side { get; set; } = side;
    public string Location { get; set; } = location;
}