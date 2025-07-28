using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine;

namespace SPT.Debugging.Patches;

public class CoordinatesPatch : ModulePatch
{
    private static TextMeshProUGUI _alphaLabel;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(BaseLocalGame<EftGamePlayerOwner>),
            nameof(BaseLocalGame<EftGamePlayerOwner>.Update)
        );
    }

    [PatchPrefix]
    public static void PatchPrefix(BaseLocalGame<EftGamePlayerOwner> __instance)
    {
        if (!Input.GetKeyDown(KeyCode.LeftControl))
            return;

        if (_alphaLabel == null)
        {
            _alphaLabel = GameObject.Find("AlphaLabel").GetComponent<TextMeshProUGUI>();
            _alphaLabel.color = Color.green;
            _alphaLabel.fontSize = 22;
            _alphaLabel.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/ARIAL SDF");
        }

        var playerOwner = __instance.PlayerOwner;
        var aiming = LookingRaycast(playerOwner.Player);

        if (_alphaLabel != null)
        {
            _alphaLabel.text = $"Looking at: [{aiming.x}, {aiming.y}, {aiming.z}]";
            Logger.LogInfo(_alphaLabel.text);
        }

        var position = playerOwner.transform.position;
        var rotation = playerOwner.transform.rotation.eulerAngles;
        Logger.LogInfo(
            $"Character position: [{position.x},{position.y},{position.z}] | Rotation: [{rotation.x},{rotation.y},{rotation.z}]"
        );
    }

    public static Vector3 LookingRaycast(Player player)
    {
        try
        {
            if (player == null || player.Fireport == null)
            {
                return Vector3.zero;
            }

            Physics.Linecast(
                player.Fireport.position,
                player.Fireport.position - player.Fireport.up * 1000f,
                out var raycastHit,
                331776
            );
            return raycastHit.point;
        }
        catch (Exception e)
        {
            Logger.LogError($"Coordinate Debug raycast failed: {e.Message}");
            return Vector3.zero;
        }
    }
}