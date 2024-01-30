using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.NextObservedPlayer;
using EFT.UI;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.BTR.Patches
{
    // Fixes the BTR Bot initialization in method_1() of BTRTurretView
    //
    // Context:
    // ClientGameWorld in LiveEFT will register the server-side BTR Bot as type ObservedPlayerView and is stored in GameWorld's allObservedPlayersByID dictionary.
    // In SPT, allObservedPlayersByID is empty which results in the game never finishing the initialization of the BTR Bot which includes disabling its gun, voice and mesh renderers.
    // For now, we do dirty patches to work around the lack of ObservedPlayerView, using Player instead.
    public class BTRBotInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BTRTurretView), nameof(BTRTurretView.method_1));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref ValueTuple<ObservedPlayerView, bool> ___valueTuple_0, int btrBotId, ref bool __result)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError("[AKI-BTR] BTRBotInitPatch - GameWorld is null");
                __result = false;
                return false;
            }

            var alivePlayersList = gameWorld.AllAlivePlayersList;
            bool doesBtrBotExist = alivePlayersList.Exists(x => x.Id == btrBotId);
            if (!doesBtrBotExist)
            {
                __result = false;
                return false;
            }
            try
            {
                Player player = alivePlayersList.First(x => x.Id == btrBotId);

                Renderer[] array = player.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].enabled = false;
                }

                var aiFirearmController = player.gameObject.GetComponent<Player.FirearmController>();
                var currentWeaponPrefab = (WeaponPrefab)AccessTools.Field(aiFirearmController.GetType(), "weaponPrefab_0").GetValue(aiFirearmController);
                if (currentWeaponPrefab.RemoveChildrenOf != null)
                {
                    foreach (var text in currentWeaponPrefab.RemoveChildrenOf)
                    {
                        var transform = currentWeaponPrefab.transform.FindTransform(text);
                        transform.gameObject.SetActive(false);
                    }
                }
                foreach (var renderer in currentWeaponPrefab.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.name == "MuzzleJetCombinedMesh")
                    {
                        renderer.transform.localPosition = new Vector3(0.18f, 0f, -0.095f);
                    }
                    else
                    {
                        renderer.enabled = false;
                    }
                }

                ___valueTuple_0 = new ValueTuple<ObservedPlayerView, bool>(new ObservedPlayerView(), true);

                __result = true;
                return false;
            }
            catch
            {
                ConsoleScreen.LogError("[AKI-BTR] BtrBot initialization failed, BtrBot will be visible ingame. Check logs.");
                throw;
            }
        }
    }
}
