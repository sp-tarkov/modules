namespace Aki.SinglePlayer.Utils.ScavMode
{
    /// <summary>
    /// Allow mods to access changes made to map settings for Scav raids
    /// </summary>
    public static class ScavRaidChangesUtil
    {
        /// <summary>
        /// The reduction in the escape time for the most recently loaded map, in minutes
        /// </summary>
        public static int RaidTimeReductionMinutes { get; private set; } = 0;

        /// <summary>
        /// The reduction in the escape time for the most recently loaded map, in seconds
        /// </summary>
        public static int RaidTimeReductionSeconds => RaidTimeReductionMinutes * 60;

        /// <summary>
        /// Updates the most recent raid-time reduction so it can be accessed by mods.
        /// 
        /// This should be internal because mods shouldn't be able to call it.
        /// </summary>
        /// <param name="raidTimeReduction">The raid-time reduction applied to the most recent Scav raid, in minutes</param>
        internal static void SetRaidTimeReduction(int raidTimeReduction)
        {
            RaidTimeReductionMinutes = raidTimeReduction;
        }
    }
}
