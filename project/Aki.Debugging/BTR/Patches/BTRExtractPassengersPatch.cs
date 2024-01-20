using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    internal class BTRExtractPassengersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(VehicleBase), nameof(VehicleBase.ExtractPassengers));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = gameWorld.MainPlayer;
            var btrManager = gameWorld.GetComponent<BTRManager>();

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
                        return;
                    }

                    btrManager.OnPlayerInteractDoor(interactionBtrPacket);
                    btrView.Interaction(player, interactionBtrPacket);
                }
            }
        }
    }
}
