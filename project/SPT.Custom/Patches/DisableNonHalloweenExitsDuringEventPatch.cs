using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SPT.Custom.Patches
{
    public class DisableNonHalloweenExitsDuringEventPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotHalloweenEvent), nameof(BotHalloweenEvent.RitualCompleted));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            Random random = new Random();
            // Get all extracts the player has
            List<ExfiltrationPoint> EligiblePoints = ExfiltrationControllerClass.Instance.EligiblePoints(gameWorld.MainPlayer.Profile).ToList();
            List<ExfiltrationPoint> PointsToPickFrom = new List<ExfiltrationPoint>();

            foreach (var ExfilPoint in EligiblePoints)
            {
                if (ExfilPoint.Status == EExfiltrationStatus.RegularMode)
                {
                    // Only add extracts that we want exludes car and timed extracts i think?
                    PointsToPickFrom.Add(ExfilPoint);
                    //ConsoleScreen.Log(ExfilPoint.Settings.Name + " Added to pool");
                }
            }
            // Randomly pick a extract from the list
            int index = random.Next(PointsToPickFrom.Count);
            string selectedExtract = PointsToPickFrom[index].Settings.Name;
            //ConsoleScreen.Log(selectedExtract + " Picked for Extract");

            ExfiltrationControllerClass.Instance.EventDisableAllExitsExceptOne(selectedExtract);
        }
    }
}
