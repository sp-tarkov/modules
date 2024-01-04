using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using System;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRInteractionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            return typeof(Player).GetMethod("BtrInteraction", bindingFlags);
        }

        [PatchPostfix]
        public static void PatchPostfix(object __instance, BTRSide btr, byte placeId, EInteractionType interaction)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = (Player)__instance;

            try
            {
                var interactionBtrPacket = btr.GetInteractWithBtrPacket(placeId, interaction);
                player.UpdateInteractionCast();

                if (interactionBtrPacket.HasInteraction)
                {
                    BTRView btrView = gameWorld.BtrController.BtrView;
                    if (btrView == null)
                    {
                        throw new NullReferenceException("BtrView not found");
                    }

                    var btrManager = gameWorld.GetComponent<BTRManager>();
                    btrManager.OnPlayerInteractDoor(interactionBtrPacket);

                    btrView.Interaction(player, interactionBtrPacket);
                }
            }
            catch (Exception ex19)
            {
                UnityEngine.Debug.LogException(ex19);
            }
        }
    }
}
