using Aki.Reflection.Patching;
using EFT.UI;
using EFT.Vehicle;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Comfort.Common;
using System;
using EFT.NextObservedPlayer;

namespace Aki.Debugging.BTR.Patches
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
            return AccessTools.Method(typeof(BTRTurretView), "method_1");
        }

        [PatchPostfix]
        public static void PatchPostfix(BTRTurretView __instance, int btrBotId, ref bool __result)
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            foreach (var playerKeyValue in gameWorld.allAlivePlayersByID)
            {
                if (playerKeyValue.Value.Id == btrBotId)
                {
                    try
                    {
                        Renderer[] array = playerKeyValue.Value.GetComponentsInChildren<Renderer>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i].enabled = false;
                        }

                        var aiFirearmController = playerKeyValue.Value.gameObject.GetComponent<Player.FirearmController>();
                        var currentWeaponPrefab = (WeaponPrefab)AccessTools.Field(aiFirearmController.GetType(), "weaponPrefab_0").GetValue(aiFirearmController);
                        if (currentWeaponPrefab.RemoveChildrenOf != null)
                        {
                            foreach (var text in currentWeaponPrefab.RemoveChildrenOf)
                            {
                                var transform = currentWeaponPrefab.transform.FindTransform(text);
                                transform?.gameObject.SetActive(false);
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

                        var tuple = new ValueTuple<ObservedPlayerView, bool>(new ObservedPlayerView(), true);
                        var btrTurretViewTupleField = AccessTools.Field(__instance.GetType(), "valueTuple_0");
                        btrTurretViewTupleField.SetValue(__instance, tuple);

                        __result = true;
                        return;
                    }
                    catch
                    {
                        ConsoleScreen.LogError("[AKI-BTR] BtrBot initialization failed, BtrBot will be visible ingame. Check logs.");
                        throw;
                    }
                }
            }
        }
    }
}
