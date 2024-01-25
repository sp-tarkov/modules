using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.NextObservedPlayer;
using EFT.UI;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Reflection;

namespace Aki.Custom.BTR.Patches
{
    // Fixes the BTR Bot initialization in AttachBot() of BTRTurretView
    //
    // Context:
    // ClientGameWorld in LiveEFT will register the server-side BTR Bot as type ObservedPlayerView and is stored in GameWorld's allObservedPlayersByID dictionary.
    // In SPT, GameWorld.allObservedPlayersByID is empty which results in the game never finishing the initialization of the BTR Bot which includes disabling its gun, voice and mesh renderers.
    // For now, we do dirty patches to work around the lack of ObservedPlayerView, using Player instead.
    public class BTRBotAttachPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRTurretView), nameof(BTRTurretView.AttachBot));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BTRTurretView __instance, int btrBotId)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError("[AKI-BTR] BTRBotAttachPatch - GameWorld is null");
                return false;
            }

            var btrTurretViewTupleField = (ValueTuple<ObservedPlayerView, bool>)AccessTools.Field(__instance.GetType(), "valueTuple_0").GetValue(__instance);
            if (!btrTurretViewTupleField.Item2)
            {
                __instance.method_1(btrBotId);
                return false;
            }

            var btrController = gameWorld.BtrController;
            if (btrController == null)
            {
                Logger.LogError("[AKI-BTR] BTRBotAttachPatch - BtrController is null");
                return false;
            }

            var btrBotShooter = btrController.BotShooterBtr;
            if (btrBotShooter == null)
            {
                Logger.LogError("[AKI-BTR] BTRBotAttachPatch - BtrBotShooter is null");
                return false;
            }
            
            try
            {
                var btrBot = btrBotShooter.GetPlayer;
                var botRootTransform = __instance.BotRoot;

                btrBot.Transform.position = botRootTransform.position;

                var firearmController = btrBot.gameObject.GetComponent<Player.FirearmController>();
                var currentWeaponPrefab = (WeaponPrefab)AccessTools.Field(firearmController.GetType(), "weaponPrefab_0").GetValue(firearmController);
                currentWeaponPrefab.transform.position = botRootTransform.position;

                btrBot.PlayerBones.Weapon_Root_Anim.SetPositionAndRotation(botRootTransform.position, botRootTransform.rotation);

                return false;
            }
            catch
            {
                ConsoleScreen.LogError("[AKI-BTR] Could not finish BtrBot initialization. Check logs.");
                throw;
            }
        }
    }
}
