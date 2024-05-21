using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;

namespace SPT.Custom.BTR.Patches
{
    public class BTRExtractPassengersPatch : ModulePatch
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
                        Logger.LogError($"[SPT-BTR] BTRExtractPassengersPatch - btrView is null");
                        return;
                    }

                    btrView.Interaction(player, interactionBtrPacket);
                    btrManager.OnPlayerInteractDoor(interactionBtrPacket);
                }
            }
        }
    }
}
