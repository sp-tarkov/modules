using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.ScavMode
{
    /// <summary>
    /// Disable PMC exfil points when playing as pscav
    /// </summary>
    public class ExfilPointManagerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(GameWorld);
            var desiredMethod = desiredType.GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            // checks nothing is null otherwise woopsies happen.
            if (gameWorld == null || gameWorld.RegisteredPlayers == null || gameWorld.ExfiltrationController == null)
            {
                Logger.LogError("Unable to Find Gameworld or RegisterPlayers... Unable to Disable Extracts for Scav raid");
            }

            Player player = gameWorld.MainPlayer;

            var exfilController = gameWorld.ExfiltrationController;

            // Only disable PMC extracts if current player is a scav.
            if (player.Fraction == ETagStatus.Scav && player.Location != "hideout")
            {
                // these are PMC extracts only, scav extracts are under a different field called ScavExfiltrationPoints.
                foreach (var exfil in exfilController.ExfiltrationPoints)
                {
                    exfil.Disable();
                }
            }
        }
    }
}
