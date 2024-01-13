using Aki.Reflection.Patching;
using EFT;
using System.Reflection;
using Aki.SinglePlayer.Utils.Insurance;
using Comfort.Common;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class InsuredItemManagerStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            // Starts tracking of insured items manager
            InsuredItemManager.Instance.Init();

            // Sets PlayerScavs items to FoundInRaid
            ConfigurePlayerScavFindInRaidStatus(gameWorld.MainPlayer);            
        }

        private static void ConfigurePlayerScavFindInRaidStatus(Player player)
        {
            if (player == null || player.Profile.Side != EPlayerSide.Savage)
            {
                return;
            }

            foreach (var item in player.Profile.Inventory.AllRealPlayerItems)
            {
                item.SpawnedInSession = true;
            }
        }
    }
}
