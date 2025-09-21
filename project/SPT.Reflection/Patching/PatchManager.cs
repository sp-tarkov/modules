using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SPT.Reflection.Patching;

public class PatchManager
{
    private readonly string _patcherName;
    private readonly Harmony _harmony;
    private readonly bool _autoPatch;
    private readonly List<ModulePatch> _patches;
    private readonly ManualLogSource _logger;

    public PatchManager(BaseUnityPlugin unityPlugin)
    {
        _patcherName = unityPlugin.Info.Metadata.Name + " PatchManager";
        _harmony = new(unityPlugin.Info.Metadata.GUID);
        _patches = [];
        _logger = Logger.CreateLogSource(_patcherName);
    }

    public PatchManager(BaseUnityPlugin unityPlugin, bool autoPatch)
    {
        _patcherName = unityPlugin.Info.Metadata.Name + " PatchManager";
        _harmony = new(unityPlugin.Info.Metadata.GUID);
        _autoPatch = autoPatch;
        _patches = [];
        _logger = Logger.CreateLogSource(_patcherName);
    }

    public PatchManager(string guid, string pluginName)
    {
        _patcherName = pluginName + " PatchManager";
        _harmony = new(guid);
        _patches = [];
        _logger = Logger.CreateLogSource(_patcherName);
    }

    public PatchManager(string guid, string pluginName, bool autoPatch)
    {
        _patcherName = pluginName + " PatchManager";
        _harmony = new(guid);
        _autoPatch = autoPatch;
        _patches = [];
        _logger = Logger.CreateLogSource(_patcherName);
    }

    /// <summary>
    /// Adds a single patch
    /// </summary>
    /// <param name="patch"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddPatch(ModulePatch patch)
    {
        if (_autoPatch)
        {
            throw new InvalidOperationException("You cannot manually add patches when using auto patching");
        }

        _patches.Add(patch);
    }

    /// <summary>
    /// Adds a list of patches
    /// </summary>
    /// <param name="patchList"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddPatches(List<ModulePatch> patchList)
    {
        if (_autoPatch)
        {
            throw new InvalidOperationException("You cannot manually add patches when using auto patching");
        }

        _patches.AddRange(patchList);
    }

    /// <summary>
    ///     Retrieves a list of types from the given assembly that inherit from <see cref="ModulePatch"/>, <br/>
    /// excluding those marked with <see cref="IgnoreAutoPatchAttribute"/> and, in non-debug builds, <br/>
    /// excluding those marked with <see cref="DebugPatchAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan for patch types.</param>
    /// <returns>
    /// A list of types that inherit from <see cref="ModulePatch"/> and meet the filtering criteria.
    /// </returns>
    private List<Type> GetPatches(Assembly assembly)
    {
        List<Type> patches = [];

        var baseType = typeof(ModulePatch);
        var ignoreAttrType = typeof(IgnoreAutoPatchAttribute);

        foreach (var type in assembly.GetTypes())
        {
            if (!baseType.IsAssignableFrom(type) || type.IsAbstract)
            {
                continue;
            }

            if (type.IsDefined(ignoreAttrType, inherit: false))
            {
                continue;
            }

            // Assembly was not built in debug and this is a debug patch, skip it.
            if (!IsAssemblyDebugBuild(assembly) && type.IsDefined(typeof(DebugPatchAttribute), inherit: false))
            {
                continue;
            }

            patches.Add(type);
        }

        return patches;
    }

    /// <summary>
    ///     Enables all patches, if <see cref="_autoPatch"/> is enabled it will find them automatically
    /// </summary>
    /// <exception cref="PatchException">
    ///     Thrown if PatcherName was not set, or there are no patches found during auto patching, or there are no patches added manually.
    /// </exception>
    public void EnablePatches()
    {
        if (_autoPatch)
        {
            var patches = GetPatches(Assembly.GetCallingAssembly());

            if (patches.Count == 0)
            {
                throw new PatchException("Could not find any patches defined in the assembly during auto patching");
            }

            var successfulPatches = 0;
            foreach (var type in patches)
            {
                try
                {
                    ((ModulePatch)Activator.CreateInstance(type)).Enable(_harmony);
                    successfulPatches++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to init [{type.Name}]: {ex.Message}");
                }
            }

            _logger.LogInfo($"Enabled {successfulPatches} patches");
            return;
        }

        if (_patches.Count == 0)
        {
            throw new PatchException("There were no patches to enable");
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _patches.Count; i++)
        {
            _patches[i].Enable(_harmony);
        }
    }

    /// <summary>
    ///     Disables all patches, if <see cref="_autoPatch"/> is enabled it will find them automatically
    /// </summary>
    /// <exception cref="PatchException">
    ///     Thrown if there are no enabled patches, or no patches are found during auto patch disabling, or there were no patches added manually to disable.
    /// </exception>
    public void DisablePatches()
    {
        if (_autoPatch)
        {
            var patches = GetPatches(Assembly.GetCallingAssembly());

            if (patches.Count == 0)
            {
                throw new PatchException("Could not find any patches defined in the assembly during auto patching");
            }

            var disabledPatches = 0;
            foreach (var type in patches)
            {
                try
                {
                    ((ModulePatch)Activator.CreateInstance(type)).Disable(_harmony);
                    disabledPatches++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to disable [{type.Name}]: {ex.Message}");
                }
            }

            _logger.LogInfo($"Disabled {disabledPatches} patches");
            return;
        }

        if (_patches.Count == 0)
        {
            throw new PatchException("There were no patches to disable");
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _patches.Count; i++)
        {
            _patches[i].Disable(_harmony);
        }
    }

    /// <summary>
    /// Enables a single patch
    /// </summary>
    /// <param name="patch"></param>
    public void EnablePatch(ModulePatch patch)
    {
        patch.Enable(_harmony);
    }

    /// <summary>
    /// Disables a single patch
    /// </summary>
    /// <param name="patch"></param>
    public void DisablePatch(ModulePatch patch)
    {
        patch.Disable(_harmony);
    }

    /// <summary>
    ///     Check if an assembly is built in debug mode
    /// </summary>
    /// <param name="assembly">Assembly to check</param>
    /// <returns>True if debug mode</returns>
    private bool IsAssemblyDebugBuild(Assembly assembly)
    {
        var debugAttr = assembly.GetCustomAttribute<DebuggableAttribute>();

        return debugAttr != null && debugAttr.IsJITOptimizerDisabled;
    }
}
