using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EFT.Airdrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.RaidFix;

/// <summary>
/// Fixes Unity constantly spamming the console with "Setting linear velocity of a kinematic body is not supported" warnings after an airdrop crate spawns.
///
/// This is caused by Unity 2022, which added this warning because setting the velocity property of a kinematic RigidBody doesn't work; the error just
/// wasn't shown in previous versions. However, regularly setting the airdrop crate's velocity to zero doesn't seem necessary, so simply removing that
/// line will fix this.
/// </summary>
public class FixUnityWarningSpamFromAirdropsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // Get the target type using this field to avoid requiring a GClass reference
        var airdropOfflineServerLogicType = AccessTools
            .Field(typeof(ClientAirdrop), nameof(ClientAirdrop.OfflineServerLogic))
            .FieldType;

        return AccessTools.Method(airdropOfflineServerLogicType, "ManualUpdate");
    }

    [PatchTranspiler]
    protected static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> originalInstructions)
    {
        MethodInfo velocitySetter = AccessTools.PropertySetter(typeof(Rigidbody), nameof(Rigidbody.velocity));

        List<CodeInstruction> modifiedInstructions = new(originalInstructions);

        for (int i = 0; i < modifiedInstructions.Count; i++)
        {
            // Search for the final instruction of the "this.Rigidbody_0.velocity = Vector3.zero;" line
            if ((modifiedInstructions[i].opcode == OpCodes.Callvirt) && ((MethodInfo)modifiedInstructions[i].operand == velocitySetter))
            {
                // Remove this instruction and all previous ones related to it (one ldarg.0 and two calls)
                for (int j = i; j > i - 4; j--)
                {
                    modifiedInstructions[j].opcode = OpCodes.Nop;
                    modifiedInstructions[j].operand = null;
                }
            }
        }

        return modifiedInstructions;
    }
}
