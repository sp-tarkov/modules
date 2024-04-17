using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;

namespace Aki.PrePatch
{
    public static class AkiBotsPrePatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static int sptUsecValue = 49;
        public static int sptBearValue = 50;

        private static string _sptPluginFolder = "plugins/spt";

        public static void Patch(ref AssemblyDefinition assembly)
        {
            // Make sure the user hasn't deleted the SPT plugins folder
            string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sptPluginPath = Path.GetFullPath(Path.Combine(assemblyFolder, "..", _sptPluginFolder));
            if (!ValidateSpt(sptPluginPath))
            {
                string message = $"'{sptPluginPath}' or required plugin files not found.\n\n" +
                    "Please re-install SPT. Exiting.";
                MessageBoxHelper.Show(message, "[SPT-AKI] Missing Core Files", MessageBoxHelper.MessageBoxType.OK);
                Environment.Exit(0);
                return;
            }

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

        private static bool ValidateSpt(string sptPluginPath)
        {
            ManualLogSource logger = Logger.CreateLogSource(nameof(AkiBotsPrePatcher));

            // Validate that the SPT plugin path exists
            if (!Directory.Exists(sptPluginPath))
            {
                logger.LogError($"'{sptPluginPath}' directory not found");
                return false;
            }

            // Validate that the folder exists, and contains our plugins
            string[] sptPlugins = new string[] { "aki-core.dll", "aki-custom.dll", "aki-singleplayer.dll" };
            string[] foundPlugins = Directory.GetFiles(sptPluginPath).Select(x => Path.GetFileName(x)).ToArray();

            foreach (string plugin in sptPlugins)
            {
                if (!foundPlugins.Contains(plugin))
                {
                    return false;
                }
            }

            return true;
        }
    }
}