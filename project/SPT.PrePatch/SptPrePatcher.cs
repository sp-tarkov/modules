using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Newtonsoft.Json;

namespace SPT.PrePatch;

public static class SptPrePatcher
{
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };
    private const string _sptPluginFolder = "plugins/spt";

    private const string EnumEntriesRoute = "/singleplayer/customEnumEntries";

    private static ManualLogSource _logger = Logger.CreateLogSource(nameof(SptPrePatcher));

    public static void Patch(ref AssemblyDefinition assembly)
    {
        PerformPreValidation();
        ChangeAppDataPath(assembly);

        EnumPatcher.PatchEnums(_logger, ref assembly, GetCustomEnumEntries());
    }

    private static List<EnumEntryDefinition> GetCustomEnumEntries()
    {
        var backendUrl = GetBackendUrl();
        var requestUri = new Uri(new Uri(backendUrl.TrimEnd('/') + "/"), EnumEntriesRoute.TrimStart('/'));

        var response = WinHttpClient.GetString(requestUri);
        if (string.IsNullOrWhiteSpace(response))
        {
            throw new InvalidOperationException($"The server returned an empty response from {EnumEntriesRoute}.");
        }

        var resp = JsonConvert.DeserializeObject<List<EnumEntryDefinition>>(response);
        if (resp is null)
        {
            throw new InvalidOperationException($"The response from {EnumEntriesRoute} is null.");
        }

        return resp;
    }

    private static string GetBackendUrl()
    {
        const string configPrefix = "-config=";
        var configArgument = Environment
            .GetCommandLineArgs()
            .FirstOrDefault(argument => argument.StartsWith(configPrefix, StringComparison.OrdinalIgnoreCase));

        if (configArgument is null)
        {
            throw new InvalidOperationException("Could not find SPT's -config launch argument containing the backend URL.");
        }

        var launcherConfig = JsonConvert.DeserializeObject<LauncherConfig>(configArgument[configPrefix.Length..]);
        if (string.IsNullOrWhiteSpace(launcherConfig?.BackendUrl))
        {
            throw new InvalidOperationException("SPT's -config launch argument did not contain a backend URL.");
        }

        return launcherConfig.BackendUrl;
    }

    private static void ChangeAppDataPath(AssemblyDefinition assembly)
    {
        // Change icon cache folder path to be local to SPT
        // find the type that contains a method called ClearIconCache, there is currently only one
        var typeToEdit = assembly.MainModule.GetTypes().FirstOrDefault(x => x.Methods.Any(m => m.Name == "ClearIconCache"));

        // find the .cctor and change the instructions to use our path instead
        var methodToEdit = typeToEdit.Methods.FirstOrDefault(x => x.Name == ".cctor");
        var ilProc = methodToEdit.Body.GetILProcessor();
        var instructions = GetCacheInstructions(assembly);

        // all this constructor does is set this static field up
        methodToEdit.Body.Instructions.Clear();

        foreach (var ins in instructions)
        {
            ilProc.Append(ins);
        }
    }

    private static List<Instruction> GetCacheInstructions(AssemblyDefinition assembly)
    {
        return new List<Instruction>
        {
            Instruction.Create(OpCodes.Call, assembly.MainModule.ImportReference(typeof(Environment).GetMethod("get_CurrentDirectory"))),
            Instruction.Create(OpCodes.Ldstr, "SPT_Runtime"),
            Instruction.Create(OpCodes.Ldstr, "user"),
            Instruction.Create(OpCodes.Ldstr, "sptappdata"),
            Instruction.Create(
                OpCodes.Call,
                assembly.MainModule.ImportReference(
                    typeof(Path).GetMethod("Combine", new[] { typeof(string), typeof(string), typeof(string), typeof(string) })
                )
            ),
            Instruction.Create(
                OpCodes.Stsfld,
                assembly
                    .MainModule.GetTypes()
                    .FirstOrDefault(x => x.Methods.Any(m => m.Name == "ClearIconCache"))
                    .Fields.FirstOrDefault(f => f.Name == "Path")
            ),
            Instruction.Create(OpCodes.Ret),
        };
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
            MessageBoxHelper.Show(errorMessage, $"[SPT] {errorTitle}", MessageBoxHelper.MessageBoxType.OK);
            Environment.Exit(0);
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

        message = "Please start SPT using SPT.Launcher.exe. Exiting.";
        return false;
    }

    private static bool ValidateSptPlugins(string sptPluginPath, out string message)
    {
        string exitMessage = "\n\nPlease re-install SPT. Exiting.";

        // Validate that the SPT plugin path exists
        if (!Directory.Exists(sptPluginPath))
        {
            message = $"'{sptPluginPath}' directory not found{exitMessage}";
            _logger.LogError(message);
            return false;
        }

        // Validate that the folder exists, and contains our plugins
        string[] sptPlugins = new string[]
        {
            "spt-common.dll",
            "spt-reflection.dll",
            "spt-core.dll",
            "spt-custom.dll",
            "spt-singleplayer.dll",
        };
        string[] foundPlugins = Directory.GetFiles(sptPluginPath).Select(x => Path.GetFileName(x)).ToArray();

        foreach (string pluginNameAndSuffix in sptPlugins)
        {
            if (!foundPlugins.Contains(pluginNameAndSuffix))
            {
                message = $"Required SPT plugin: {pluginNameAndSuffix} missing from '{sptPluginPath}' {exitMessage}";
                _logger.LogError(message);
                return false;
            }
        }

        message = "";
        return true;
    }

    private sealed class LauncherConfig
    {
        public string BackendUrl { get; set; }
    }
}
