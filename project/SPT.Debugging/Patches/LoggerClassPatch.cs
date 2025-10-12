using System;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using NLog;
using SPT.Common.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SPT.Debugging.Patches;

public class LoggerClassLogPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools
            .GetDeclaredMethods(typeof(LoggerClass))
            .SingleCustom(m => m.Name == nameof(LoggerClass.Log) && m.GetParameters().Length == 4);
    }

    [PatchPostfix]
    public static void PatchPostfix(string nlogFormat, string unityFormat, LogLevel logLevel, object[] args)
    {
        var bsgLevel = LogLevel.FromOrdinal(logLevel.Ordinal);
        var sptLevel = LogLevel.FromOrdinal(SPTDebuggingPlugin.logLevel.verbosity);

        // See Nlog docs for information on ordinal levels
        // Ordinal works from low to high 0 - trace, 1 - debug, 3 - info ...
        if (bsgLevel >= sptLevel)
        {
            // First replace all { and } with {{ and }}
            nlogFormat = nlogFormat.Replace("{", "{{");
            nlogFormat = nlogFormat.Replace("}", "}}");

            // Then find any instance of "{{\d}}" and unescape its brackets
            nlogFormat = Regex.Replace(nlogFormat, @"{{(\d+)}}", "{$1}");

            try
            {
                nlogFormat = string.Format(nlogFormat, args);
            }
            catch (Exception)
            {
                Logger.LogError($"Error formatting string: {nlogFormat}");
                for (int i = 0; i < args.Length; i++)
                {
                    Logger.LogError($"  args[{i}] = {args[i]}");
                }
                return;
            }

            Logger.LogDebug($"output Nlog: {logLevel} : {nlogFormat}");

            if (SPTDebuggingPlugin.logLevel.sendToServer)
            {
                ServerLog.Info("EFT Logging:", $"{logLevel} : {nlogFormat}");
            }
        }

        // I've opted to leave this disabled for now, it doesn't add much in
        // terms of value, its mostly the same stuff as the nlogFormat
        // Deciced to keep it here incase we decide we want it later.
        // After a 5 minute factory run at full verbosity, i ended up with a 20K
        // line long player.log file.

        //unityFormat = Regex.Replace(unityFormat, @"\{[^{}]*[^\d{}][^{}]*\}", "");
        //unityFormat = string.Format(unityFormat, args);
        //Logger.LogDebug($"Verbosity: {logLevel}");
        //Logger.LogDebug($"output unity: {unityFormat}");
    }
}
