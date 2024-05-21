using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.NextObservedPlayer;
using EFT.Vehicle;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Aki.Custom.BTR.Patches
{
    // Fixes the BTR Bot initialization in AttachBot() of BTRTurretView
    //
    // Context:
    // ClientGameWorld in LiveEFT will register the server-side BTR Bot as type ObservedPlayerView and is stored in GameWorld's allObservedPlayersByID dictionary.
    // In SPT, GameWorld.allObservedPlayersByID is empty which results in the game never finishing the initialization of the BTR Bot which includes disabling its gun, voice and mesh renderers.
    //
    // This is essentially a full reimplementation of the BTRTurretView class, but using Player instead of ObservedPlayerView.
    //
    public class BTRBotAttachPatch : ModulePatch
    {
        private static FieldInfo _valueTuple0Field;
        private static FieldInfo _gunModsToDisableField;
        private static FieldInfo _weaponPrefab0Field;
        private static readonly List<Renderer> rendererList = new List<Renderer>(256);

        protected override MethodBase GetTargetMethod()
        {
            var targetType = typeof(BTRTurretView);

            _valueTuple0Field = AccessTools.Field(targetType, "valueTuple_0");
            _gunModsToDisableField = AccessTools.Field(targetType, "_gunModsToDisable");
            _weaponPrefab0Field = AccessTools.Field(typeof(Player.FirearmController), "weaponPrefab_0");

            return AccessTools.Method(targetType, nameof(BTRTurretView.AttachBot));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BTRTurretView __instance, int btrBotId)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                return false;
            }

            // Find the BTR turret
            var alivePlayersList = gameWorld.AllAlivePlayersList;
            Player turretPlayer = alivePlayersList.FirstOrDefault(x => x.Id == btrBotId);
            if (turretPlayer == null)
            {
                return false;
            }

            // Init the turret view
            var valueTuple = (ValueTuple<ObservedPlayerView, bool>)_valueTuple0Field.GetValue(__instance);
            if (!valueTuple.Item2 && !InitTurretView(__instance, turretPlayer))
            {
                Logger.LogError("[SPT-BTR] BTRBotAttachPatch - BtrBot initialization failed");
                return false;
            }

            WeaponPrefab weaponPrefab;
            Transform transform;
            if (FindTurretObjects(turretPlayer, out weaponPrefab, out transform))
            {
                weaponPrefab.transform.SetPositionAndRotation(__instance.GunRoot.position, __instance.GunRoot.rotation);
                transform.SetPositionAndRotation(__instance.GunRoot.position, __instance.GunRoot.rotation);
            }

            return false;
        }

        private static bool InitTurretView(BTRTurretView btrTurretView, Player turretPlayer)
        {
            EnableTurretObjects(btrTurretView, turretPlayer, false);

            // We only use this for tracking whether the turret is initialized, so we don't need to set the ObservedPlayerView
            _valueTuple0Field.SetValue(btrTurretView, new ValueTuple<ObservedPlayerView, bool>(null, true));
            return true;
        }

        private static void EnableTurretObjects(BTRTurretView btrTurretView, Player player, bool enable)
        {
            // Find the turret weapon transform
            WeaponPrefab weaponPrefab;
            Transform weaponTransform;
            if (!FindTurretObjects(player, out weaponPrefab, out weaponTransform))
            {
                return;
            }

            // Hide the turret bot
            SetVisible(player, weaponPrefab, false);

            // Disable the components we need to disaable
            var _gunModsToDisable = (string[])_gunModsToDisableField.GetValue(btrTurretView);
            foreach (Transform child in weaponTransform)
            {
                if (_gunModsToDisable.Contains(child.name))
                {
                    child.gameObject.SetActive(enable);
                }
            }
        }

        private static bool FindTurretObjects(Player player, out WeaponPrefab weaponPrefab, out Transform weapon)
        {
            // Find the WeaponPrefab and Transform of the turret weapon
            var aiFirearmController = player.gameObject.GetComponent<Player.FirearmController>();
            weaponPrefab = (WeaponPrefab)_weaponPrefab0Field.GetValue(aiFirearmController);

            if (weaponPrefab == null)
            {
                weapon = null;
                return false;
            }

            weapon = weaponPrefab.Hierarchy.GetTransform(ECharacterWeaponBones.weapon);
            return weapon != null;
        }

        /**
         * A re-implementation of the ObservedPlayerController.Culling.Mode setter that works for a Player object
         */
        private static void SetVisible(Player player, WeaponPrefab weaponPrefab, bool isVisible)
        {
            // Toggle any animators and colliders
            if (player.HealthController.IsAlive)
            {
                IAnimator bodyAnimatorCommon = player.GetBodyAnimatorCommon();
                if (bodyAnimatorCommon.enabled != isVisible)
                {
                    bool flag = !bodyAnimatorCommon.enabled;
                    bodyAnimatorCommon.enabled = isVisible;
                    FirearmsAnimator firearmsAnimator = player.HandsController.FirearmsAnimator;
                    if (firearmsAnimator != null && firearmsAnimator.Animator.enabled != isVisible)
                    {
                        firearmsAnimator.Animator.enabled = isVisible;
                    }
                }

                PlayerPoolObject component = player.gameObject.GetComponent<PlayerPoolObject>();
                foreach (Collider collider in component.Colliders)
                {
                    if (collider.enabled != isVisible)
                    {
                        collider.enabled = isVisible;
                    }
                }
            }

            // Build a list of renderers for this player object and set their rendering state
            rendererList.Clear();
            player.PlayerBody.GetRenderersNonAlloc(rendererList);
            if (weaponPrefab != null)
            {
                rendererList.AddRange(weaponPrefab.Renderers);
            }
            rendererList.ForEach(renderer => renderer.forceRenderingOff = !isVisible);
        }
    }
}
