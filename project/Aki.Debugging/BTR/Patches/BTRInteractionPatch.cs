using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    public class BTRInteractionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(Player), IsTargetMethod);
        }

        /**
         * Find the "BtrInteraction" method that takes parameters
         */
        private bool IsTargetMethod(MethodBase method)
        {
            return method.Name == nameof(Player.BtrInteraction) && method.GetParameters().Length > 0;
        }

        [PatchPostfix]
        public static void PatchPostfix(object __instance, BTRSide btr, byte placeId, EInteractionType interaction)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var player = (Player)__instance;
            var btrManager = gameWorld.GetComponent<BTRManager>();

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
