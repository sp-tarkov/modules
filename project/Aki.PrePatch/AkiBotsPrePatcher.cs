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

        public static int sptUsecValue = 47;
        public static int sptBearValue = 48;

        private static string _sptPluginFolder = "BepInEx/plugins/spt";

        public static void Patch(ref AssemblyDefinition assembly)
        {
            // Make sure the user hasn't deleted the SPT plugins folder
            if (!ValidateSpt())
            {
                string message = $"`{_sptPluginFolder}` or required plugin files not found.\n" +
                    "Please re-install SPT.\n" +
                    "Exiting.";
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

        private static bool ValidateSpt()
        {
            ManualLogSource logger = Logger.CreateLogSource(nameof(AkiBotsPrePatcher));

            // Get the directory our prepatcher is in
            string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Validate that the SPT plugin path exists
            string sptPluginPath = Path.GetFullPath(Path.Combine(assemblyFolder, "..", _sptPluginFolder));
            if (!Directory.Exists(sptPluginPath))
            {
                logger.LogError($"`{_sptPluginFolder}` directory not found");
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