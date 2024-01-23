using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents;
using EFT.Vehicle;
using HarmonyLib;
using System.Reflection;
using GlobalEventHandler = GClass2909;

namespace Aki.Custom.BTR.Patches
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
        private static void PatchPostfix(Player __instance, BTRSide btr, byte placeId, EInteractionType interaction)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var btrManager = gameWorld.GetComponent<BTRManager>();

            var interactionBtrPacket = btr.GetInteractWithBtrPacket(placeId, interaction);
            __instance.UpdateInteractionCast();

            // Prevent player from entering BTR when blacklisted
            var btrBot = gameWorld.BtrController.BotShooterBtr;
            if (btrBot.BotsGroup.Enemies.ContainsKey(__instance))
            {
                // Notify player they are blacklisted from entering BTR
                GlobalEventHandler.CreateEvent<BtrNotificationInteractionMessageEvent>().Invoke(__instance.Id, EBtrInteractionStatus.Blacklisted);
                return;
            }

            if (interactionBtrPacket.HasInteraction)
            {
                BTRView btrView = gameWorld.BtrController.BtrView;
                if (btrView == null)
                {
                    return;
                }

                btrManager.OnPlayerInteractDoor(interactionBtrPacket);
                btrView.Interaction(__instance, interactionBtrPacket);
            }
        }
    }
}
