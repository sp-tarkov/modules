using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace Aki.SinglePlayer.Patches.ScavMode
{
    /// <summary>
    /// Disable PMC exfil points when playing as pscav
    /// </summary>
    public class ExfilPointManagerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            // checks nothing is null otherwise bad things happen
            if (gameWorld == null || gameWorld.RegisteredPlayers == null || gameWorld.ExfiltrationController == null)
            {
                Logger.LogError("Could not find GameWorld or RegisterPlayers... Unable to disable extracts for Scav raid");
            }

            Player player = gameWorld.MainPlayer;

            // Only disable PMC extracts if current player is a scav
            if (player.Fraction == ETagStatus.Scav && player.Location != "hideout")
            {
                // these are PMC extracts only, scav extracts are under a different field called ScavExfiltrationPoints
                foreach (var exfil in gameWorld.ExfiltrationController.ExfiltrationPoints)
                {
                    exfil.Disable();
                }
            }
        }
    }
}
