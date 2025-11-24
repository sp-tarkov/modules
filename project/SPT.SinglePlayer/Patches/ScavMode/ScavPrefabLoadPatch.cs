using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.CodeWrapper;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.SinglePlayer.Patches.ScavMode;

public class ScavPrefabLoadPatch : ModulePatch
{
    /// <summary>
    /// Aim of this patch is to check what Side we are loading into the raid as.
    /// then if we are Scav, the game will load the bundles for that
    /// else it will load the bundles for PMC
    /// </summary>
    /// <returns></returns>
    protected override MethodBase GetTargetMethod()
    {
        // Struct324 - 3.10.0
        var desiredType = typeof(TarkovApplication)
            .GetNestedTypes(PatchConstants.PublicDeclaredFlags)
            .SingleCustom(x =>
                x.GetField("timeAndWeather") != null
                && x.GetField("tarkovApplication_0") != null
                && x.GetField("inTransition") != null
                && x.Name.Contains("Struct")
            );

        var desiredMethod = desiredType.GetMethods(PatchConstants.PublicDeclaredFlags).FirstOrDefault(x => x.Name == "MoveNext");

        Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
        Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

        return desiredMethod;
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> PatchTranspile(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        // Search for code where backend.Session.getProfile() is called.
        var searchCode = new CodeInstruction(
            OpCodes.Callvirt,
            AccessTools.Method(PatchConstants.BackendProfileInterfaceType, "get_Profile")
        );
        var searchIndex = -1;

        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == searchCode.opcode && codes[i].operand == searchCode.operand)
            {
                searchIndex = i;
                break;
            }
        }

        // Patch failed.
        if (searchIndex == -1)
        {
            Logger.LogError($"Patch {MethodBase.GetCurrentMethod()} failed: Could not find reference code.");
            return instructions;
        }

        // Move back by 2. This is the start of IL chain that we're interested in.
        searchIndex -= 2;

        var brFalseLabel = generator.DefineLabel();
        var brLabel = generator.DefineLabel();

        var newCodes = CodeGenerator.GenerateInstructions(
            new List<Code>()
            {
                new Code(OpCodes.Ldloc_1),
                new Code(OpCodes.Call, typeof(ClientApplication<IClientSession>), "get_Session"),
                new Code(OpCodes.Ldloc_1),
                new Code(OpCodes.Ldfld, typeof(TarkovApplication), "_raidSettings"),
                new Code(OpCodes.Callvirt, typeof(RaidSettings), "get_IsPmc"),
                new Code(OpCodes.Brfalse, brFalseLabel),
                new Code(OpCodes.Callvirt, PatchConstants.BackendProfileInterfaceType, "get_Profile"),
                new Code(OpCodes.Br, brLabel),
                new CodeWithLabel(OpCodes.Callvirt, brFalseLabel, PatchConstants.BackendProfileInterfaceType, "get_ProfileOfPet"),
                new CodeWithLabel(OpCodes.Ldc_I4_1, brLabel),
            }
        );

        codes.RemoveRange(searchIndex, 4);
        codes.InsertRange(searchIndex, newCodes);

        return codes.AsEnumerable();
    }
}
