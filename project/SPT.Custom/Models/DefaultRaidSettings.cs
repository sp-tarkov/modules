using EFT.Bots;

namespace SPT.Custom.Models
{
    public class DefaultRaidSettings
    {
        public EBotAmount AiAmount;
        public EBotDifficulty AiDifficulty;
        public bool BossEnabled;
        public bool ScavWars;
        public bool TaggedAndCursed;
        public bool EnablePve;
        public bool RandomWeather;
        public bool RandomTime;

        public DefaultRaidSettings(EBotAmount aiAmount, EBotDifficulty aiDifficulty, bool bossEnabled, bool scavWars, bool taggedAndCursed, bool enablePve, bool randomWeather, bool randomTime)
        {
            AiAmount = aiAmount;
            AiDifficulty = aiDifficulty;
            BossEnabled = bossEnabled;
            ScavWars = scavWars;
            TaggedAndCursed = taggedAndCursed;
            EnablePve = enablePve;
            RandomWeather = randomWeather;
            RandomTime = randomTime;
        }
    }
}
