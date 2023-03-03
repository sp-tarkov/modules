using EFT.Bots;

namespace Aki.Custom.Models
{
    public class DefaultRaidSettings
    {
        public EBotAmount AiAmount;
        public EBotDifficulty AiDifficulty;
        public bool BossEnabled;
        public bool ScavWars;
        public bool TaggedAndCursed;
        public bool EnablePve;

        public DefaultRaidSettings(EBotAmount aiAmount, EBotDifficulty aiDifficulty, bool bossEnabled, bool scavWars, bool taggedAndCursed, bool enablePve)
        {
            AiAmount = aiAmount;
            AiDifficulty = aiDifficulty;
            BossEnabled = bossEnabled;
            ScavWars = scavWars;
            TaggedAndCursed = taggedAndCursed;
            EnablePve = enablePve;
        }
    }
}
