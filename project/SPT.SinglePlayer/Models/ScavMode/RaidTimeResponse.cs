using System.Collections.Generic;

namespace SPT.SinglePlayer.Models.ScavMode
{
    public class RaidTimeResponse
    {
        public int raidTimeMinutes { get; set; }
        public int? newSurviveTimeSeconds { get; set; }
        public int originalSurvivalTimeSeconds { get; set; }
        public List<ExitChanges> exitChanges { get; set; }
        
    }

    public class ExitChanges
    {
        public string Name{ get; set; }
        public int? MinTime { get; set; }
        public int? MaxTime { get; set; }
        public int? Chance { get; set; }
    }
}