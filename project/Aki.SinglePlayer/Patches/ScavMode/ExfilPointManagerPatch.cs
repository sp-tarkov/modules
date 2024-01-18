using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using System.Reflection;
using HarmonyLib;
using EFT.Interactive;
using System.Linq;

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
                foreach (var exfil in gameWorld.ExfiltrationController.ExfiltrationPoints)
                {
                    if (exfil is ScavExfiltrationPoint scavExfil)
                    {
                        // We are checking if player exists in list so we dont disable the wrong extract
                        if(!scavExfil.EligibleIds.Contains(player.ProfileId))
                        {
                            exfil.Disable();
                        }
                    }
                    else
                    {
                        // Disabling extracts that aren't scav extracts
                        exfil.Disable();
                        // _authorityToChangeStatusExternally Changing this to false stop buttons from re-enabling extracts (d-2 extract, zb-013)
                        exfil.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "_authorityToChangeStatusExternally").SetValue(exfil, false);
                    }
                }
            }
        }
    }
}
