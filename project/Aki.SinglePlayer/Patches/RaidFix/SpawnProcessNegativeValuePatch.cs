using Aki.Common.Http;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Prevent BotSpawnerClass from adjusting the spawn process value to be below 0
    /// This fixes aiamount = high spawning 80+ bots on maps like streets/customs
    /// int_0 = all bots alive
    /// int_1 = followers alive
    /// int_2 = bosses currently alive
    /// int_3 = spawn process? - current guess is open spawn positions - bsg doesnt seem to handle negative vaues well
    /// int_4 = max bots
    /// </summary>
    class SpawnProcessNegativeValuePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(BotSpawnerClass);
            var desiredMethod = desiredType.GetMethod("CheckOnMax", PatchConstants.PublicFlags);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        private static void PatchPreFix(ref int ____maxBots)
        {
            // Spawn process
            if (____maxBots < 0)
            {
                ____maxBots = 0;
            }
        }
    }
}