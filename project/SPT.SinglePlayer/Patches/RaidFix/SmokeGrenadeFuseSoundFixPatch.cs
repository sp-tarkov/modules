using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using HarmonyLib;
using UnityEngine;

namespace SPT.SinglePlayer.Patches.RaidFix
{
    /// <summary>
    /// Fixes an issue with smoke grenades not playing the fuse popping sound when thrown
    /// </summary>
    public class SmokeGrenadeFuseSoundFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GrenadeEmission), nameof(GrenadeEmission.StartEmission));
        }

        [PatchTranspiler]
        public static IEnumerable<CodeInstruction> PatchTranspile(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var searchCode = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(AudioClip), "get_length"));
            var searchIndex = -1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
                {
                    searchIndex = i;
                    break;
                }
            }

            if (searchIndex == -1)
            {
                Logger.LogError($"{nameof(SmokeGrenadeFuseSoundFixPatch)} failed: Could not find reference code.");
                return instructions;
            }

            var newCodes = CodeGenerator.GenerateInstructions(new List<Code>
            {
                new Code(OpCodes.Ldarg_0),
                new Code(OpCodes.Ldfld, typeof(GrenadeEmission), "betterSource_0"),
                new Code(OpCodes.Ldfld, typeof(BetterSource), "source1"),
                new Code(OpCodes.Callvirt, typeof(AudioSource), "Play")
            });

            searchIndex -= 4;
            
            codes.InsertRange(searchIndex, newCodes);
            return codes.AsEnumerable();
        }
    }
}