using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRExtractPassengersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(VehicleBase), nameof(VehicleBase.ExtractPassengers));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld.MainPlayer;
            var btrManager = gameWorld.GetComponent<BTRManager>();

            try
            {
                var btrSide = btrManager.LastInteractedBtrSide;
                if (btrSide == null)
                {
                    return;
                }

                if (btrSide.TryGetCachedPlace(out byte b))
                {
                    var interactionBtrPacket = btrSide.GetInteractWithBtrPacket(b, EInteractionType.GoOut);
                    if (interactionBtrPacket.HasInteraction)
                    {
                        BTRView btrView = gameWorld.BtrController.BtrView;
                        if (btrView == null)
                        {
                            throw new NullReferenceException("BtrView not found");
                        }

                        btrManager.OnPlayerInteractDoor(interactionBtrPacket);
                        btrView.Interaction(player, interactionBtrPacket);
                    }
                }
            }
            catch (Exception ex19)
            {
                UnityEngine.Debug.LogException(ex19);
            }
        }
    }
}
