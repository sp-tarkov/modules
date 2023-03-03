using System.Collections.Generic;
using Mono.Cecil;

namespace Aki.PrePatch
{
    public static class AkiBotsPrePatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static long sptUsecValue = 0x80000000;
        public static long sptBearValue = 0x100000000;

        public static void Patch(ref AssemblyDefinition assembly)
        {
            var botEnums = assembly.MainModule.GetType("EFT.WildSpawnType");

            var sptUsec = new FieldDefinition("sptUsec",
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                    botEnums)
                { Constant = sptUsecValue };

            var sptBear = new FieldDefinition("sptBear",
                    FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                    botEnums)
                { Constant = sptBearValue };

            botEnums.Fields.Add(sptUsec);
            botEnums.Fields.Add(sptBear);
        }
    }
}