using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using TMPro;
using UnityEngine;
using System;
using System.Reflection;

namespace Aki.Debugging.Patches
{
    public class CoordinatesPatch : ModulePatch
    {
        private static TextMeshProUGUI _alphaLabel;
        private static PropertyInfo _playerProperty;

        protected override MethodBase GetTargetMethod()
        {
            var localGameBaseType = PatchConstants.LocalGameType.BaseType;
            _playerProperty = localGameBaseType.GetProperty("PlayerOwner", BindingFlags.Public | BindingFlags.Instance);
            return localGameBaseType.GetMethod("Update", PatchConstants.PrivateFlags);
        }

        [PatchPrefix]
        private static void PatchPrefix(object __instance)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (_alphaLabel == null)
                {
                    _alphaLabel = GameObject.Find("AlphaLabel").GetComponent<TextMeshProUGUI>();
                    _alphaLabel.color = Color.green;
                    _alphaLabel.fontSize = 22;
                    _alphaLabel.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/ARIAL SDF");
                }

                var playerOwner = (GamePlayerOwner)_playerProperty.GetValue(__instance);
                var aiming = LookingRaycast(playerOwner.Player);

                if (_alphaLabel != null)
                {
                    _alphaLabel.text = $"Looking at: [{aiming.x}, {aiming.y}, {aiming.z}]";
                    Logger.LogInfo(_alphaLabel.text);
                }

                var position = playerOwner.transform.position;
                var rotation = playerOwner.transform.rotation.eulerAngles;
                Logger.LogInfo($"Character position: [{position.x},{position.y},{position.z}] | Rotation: [{rotation.x},{rotation.y},{rotation.z}]");
            }
        }

        public static Vector3 LookingRaycast(Player player)
        {
            try
            {
                if (player == null || player.Fireport == null)
                {
                    return Vector3.zero;
                }

                Physics.Linecast(player.Fireport.position, player.Fireport.position - player.Fireport.up * 1000f, out var raycastHit, 331776);
                return raycastHit.point;
            }
            catch (Exception e)
            {
                Logger.LogError($"Coordinate Debug raycast failed: {e.Message}");
                return Vector3.zero;
            }
        }
    }
}
