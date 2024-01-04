using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.NextObservedPlayer;
using EFT.UI;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Reflection;

namespace Aki.Debugging.BTR.Patches
{
    // Fixes the BTR Bot initialization in AttachBot() of BTRTurretView
    //
    // Context:
    // ClientGameWorld in LiveEFT will register the server-side BTR Bot as type ObservedPlayerView and is stored in GameWorld's allObservedPlayersByID dictionary.
    // In SPT, GameWorld.allObservedPlayersByID is empty which results in the game never finishing the initialization of the BTR Bot which includes disabling its gun, voice and mesh renderers.
    // Perhaps some research should be done into getting the dictionary populated as ObservedPlayerView seems to be utilised by several aspects of the BTR's functionality.
    // For now, we do dirty patches to work around the lack of ObservedPlayerView, using Player instead.
    public class BTRBotAttachPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRTurretView), nameof(BTRTurretView.AttachBot));
        }

        [PatchPrefix]
        public static bool PatchPrefix(object __instance, int btrBotId)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var btrTurretView = (BTRTurretView)__instance;

            var btrTurretViewTupleField = (ValueTuple<ObservedPlayerView, bool>)AccessTools.Field(btrTurretView.GetType(), "valueTuple_0")
                .GetValue(btrTurretView);

            if (!btrTurretViewTupleField.Item2)
            {
                var btrTurretViewMethod = AccessTools.Method(btrTurretView.GetType(), "method_1");
                btrTurretViewMethod.Invoke(btrTurretView, new object[] { btrBotId });
            }
            if (!btrTurretViewTupleField.Item2)
            {
                return false;
            }

            var btrBot = gameWorld.BtrController.BotShooterBtr?.GetPlayer;
            if (btrBot == null)
            {
                return false;
            }
            try
            {
                var botRootTransform = btrTurretView.BotRoot;

                btrBot.Transform.position = botRootTransform.position;

                var aiFirearmController = btrBot.gameObject.GetComponent<Player.FirearmController>();
                var currentWeaponPrefab = (WeaponPrefab)AccessTools.Field(aiFirearmController.GetType(), "weaponPrefab_0").GetValue(aiFirearmController);
                currentWeaponPrefab.transform.position = botRootTransform.position;

                btrBot.PlayerBones.Weapon_Root_Anim.SetPositionAndRotation(botRootTransform.position, botRootTransform.rotation);

                return false;
            }
            catch
            {
                ConsoleScreen.LogError("[AKI-BTR]: Could not finish BtrBot initialization. Check logs.");
                throw;
            }
        }
    }
}
