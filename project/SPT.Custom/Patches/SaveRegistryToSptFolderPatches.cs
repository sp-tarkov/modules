using HarmonyLib;
using Newtonsoft.Json.Linq;
using SPT.Reflection.Patching;
using System;
using System.IO;
using System.Reflection;
using EFT.UI;
using UnityEngine;

namespace SPT.Custom.Patches
{
    /// <summary>
    /// Redirect registry reads/writes to a folder in the SPT directory, instead of sharing
    /// registry entries with live.
    /// 
    /// Note this is a multi-patch to keep these patches grouped together, as we need to patch
    /// many methods to properly implement this
    /// </summary>
    public class SaveRegistryToSptFolderPatches
    {
        private static readonly string _sptRegistryPath = Path.Combine(Environment.CurrentDirectory, "user", "sptRegistry");
        private static readonly string _registryFilePath = Path.Combine(_sptRegistryPath, "registry.json");
        private static JObject _sptRegistry = new JObject();

        public void Enable()
        {
            Init();

            new PatchPlayerPrefsSetInt().Enable();
            new PatchPlayerPrefsSetFloat().Enable();
            new PatchPlayerPrefsSetString().Enable();
            new PatchPlayerPrefsGetInt().Enable();
            new PatchPlayerPrefsGetFloat().Enable();
            new PatchPlayerPrefsGetString().Enable();
            new PatchPlayerPrefsHasKey().Enable();
            new PatchPlayerPrefsDeleteKey().Enable();
            new PatchPlayerPrefsDeleteAll().Enable();
            new PatchPlayerPrefsSave().Enable();
        }

        public void Init()
        {
            // Make sure the registry directory exists
            if (!Directory.Exists(_sptRegistryPath))
            {
                Directory.CreateDirectory(_sptRegistryPath);
            }

            
            if (!File.Exists(_registryFilePath))
            {
                return;
            }

            try
            {
                // Load existing registry
                _sptRegistry = JObject.Parse(File.ReadAllText(_registryFilePath));
            }
            catch (Exception e)
            {
                ConsoleScreen.LogError($"Unable to parse registry file, defaulting to empty: {e.Message}");
            }

            // Make sure we save the registry on exit, for some reason this isn't triggering by Unity itself
            Application.quitting += PlayerPrefs.Save;
        }

        public class PatchPlayerPrefsSetInt : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.SetInt));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(string key, int value)
            {
                _sptRegistry[key] = value;
                return false;
            }
        }

        public class PatchPlayerPrefsSetFloat : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.SetFloat));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(string key, float value)
            {
                _sptRegistry[key] = value;
                return false;
            }
        }

        public class PatchPlayerPrefsSetString : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.SetString));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(string key, string value)
            {
                _sptRegistry[key] = value;
                return false;
            }
        }

        public class PatchPlayerPrefsGetInt : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.GetInt), [typeof(string), typeof(int)]);
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(ref int __result, string key, int defaultValue)
            {
                if (_sptRegistry.TryGetValue(key, out var value))
                {
                    __result = value.Value<int>();
                }
                else
                {
                    __result = defaultValue;
                }
                return false;
            }
        }

        public class PatchPlayerPrefsGetFloat : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.GetFloat), [typeof(string), typeof(float)]);
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(ref float __result, string key, float defaultValue)
            {
                if (_sptRegistry.TryGetValue(key, out var value))
                {
                    __result = value.Value<float>();
                }
                else
                {
                    __result = defaultValue;
                }
                return false;
            }
        }

        public class PatchPlayerPrefsGetString : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.GetString), [typeof(string), typeof(string)]);
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(ref string __result, string key, string defaultValue)
            {
                if (_sptRegistry.TryGetValue(key, out var value))
                {
                    __result = value.Value<string>();
                }
                else
                {
                    __result = defaultValue;
                }
                return false;
            }
        }

        public class PatchPlayerPrefsHasKey : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.HasKey));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(ref bool __result, string key)
            {
                __result = _sptRegistry.ContainsKey(key);
                return false;
            }
        }

        public class PatchPlayerPrefsDeleteKey : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.DeleteKey));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix(string key)
            {
                _sptRegistry.Remove(key);
                return false;
            }
        }

        public class PatchPlayerPrefsDeleteAll : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.DeleteAll));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix()
            {
                _sptRegistry.RemoveAll();
                return false;
            }
        }

        public class PatchPlayerPrefsSave : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                MethodInfo method = AccessTools.Method(typeof(PlayerPrefs), nameof(PlayerPrefs.Save));
                return method;
            }

            [PatchPrefix]
            private static bool PatchPrefix()
            {
                File.WriteAllText(_registryFilePath, _sptRegistry.ToString());
                return false;
            }
        }

    }
}
