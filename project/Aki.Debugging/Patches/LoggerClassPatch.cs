using System.Reflection;
using System.Text.RegularExpressions;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using HarmonyLib;
using NLog;

namespace Aki.Debugging.Patches
{
    public class LoggerClassLogPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(LoggerClass))
                .SingleCustom(m => m.Name == nameof(LoggerClass.Log) && m.GetParameters().Length == 4);
        }

        [PatchPostfix]
        private static void PatchPostfix(string nlogFormat, string unityFormat, LogLevel logLevel, object[] args)
        {
            var bsgLevel = LogLevel.FromOrdinal(logLevel.Ordinal);
            var sptLevel = LogLevel.FromOrdinal(AkiDebuggingPlugin.logLevel.verbosity);

            // See Nlog docs for information on ordinal levels
            // Ordinal works from low to high 0 - trace, 1 - debug, 3 - info ...
            if (bsgLevel >= sptLevel)
            {
                // We want to remove any character thats not a single digit inside of {}
                // This prevents string builder exceptions.
                nlogFormat = Regex.Replace(nlogFormat, @"\{[^{}]*[^\d{}][^{}]*\}", "");
                nlogFormat = string.Format(nlogFormat, args);

                Logger.LogDebug($"output Nlog: {logLevel} : {nlogFormat}");
                
                if (AkiDebuggingPlugin.logLevel.sendToServer)
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
}