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

        public static int sptUsecValue = 51;
        public static int sptBearValue = 52;

        private static string _sptPluginFolder = "plugins/spt";

        private static string _sptPluginFolder = "plugins/spt";

        public static void Patch(ref AssemblyDefinition assembly)
        {
            PerformPreValidation();

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

        private static void PerformPreValidation()
        {
            // Check if the launcher was used
            bool launcherUsed = ValidateLauncherUse(out string launcherError);

            // Check that all the expected plugins are in the BepInEx/Plugins/spt/ folder
            string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sptPluginPath = Path.GetFullPath(Path.Combine(assemblyFolder, "..", _sptPluginFolder));
            bool pluginsValidated = ValidateSptPlugins(sptPluginPath, out string pluginErrorMessage);

            // If either the launcher wasn't used, or the plugins weren't found, exit
            if (!launcherUsed || !pluginsValidated)
            {
                string errorTitle = (!launcherUsed) ? "Startup Error" : "Missing Core Files";
                string errorMessage = (!launcherUsed) ? launcherError : pluginErrorMessage;
                MessageBoxHelper.Show(errorMessage, $"[SPT-AKI] {errorTitle}", MessageBoxHelper.MessageBoxType.OK);
                Environment.Exit(0);
                return;
            }
        }

        private static bool ValidateLauncherUse(out string message)
        {
            // Validate that parameters were passed to EscapeFromTarkov.exe, to verify the
            // player used the SPT Launcher to start the process
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                message = "";
                return true;
            }

            message = "Please start SPT-AKI using Aki.Launcher.exe. Exiting.";
            return false;
        }

        private static bool ValidateSptPlugins(string sptPluginPath, out string message)
        {
            string exitMessage = "\n\nPlease re-install SPT. Exiting.";
            ManualLogSource logger = Logger.CreateLogSource(nameof(AkiBotsPrePatcher));

            // Validate that the SPT plugin path exists
            if (!Directory.Exists(sptPluginPath))
            {
                message = $"'{sptPluginPath}' directory not found{exitMessage}";
                logger.LogError(message);
                return false;
            }

            // Validate that the folder exists, and contains our plugins
            string[] sptPlugins = new string[] { "aki-core.dll", "aki-custom.dll", "aki-singleplayer.dll" };
            string[] foundPlugins = Directory.GetFiles(sptPluginPath).Select(x => Path.GetFileName(x)).ToArray();

            foreach (string plugin in sptPlugins)
            {
                if (!foundPlugins.Contains(plugin))
                {
                    message = $"Required SPT plugins missing from '{sptPluginPath}'{exitMessage}";
                    logger.LogError(message);
                    return false;
                }
            }

            message = "";
            return true;
        }
    }
}