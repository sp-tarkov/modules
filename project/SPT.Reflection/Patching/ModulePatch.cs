using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace SPT.Reflection.Patching;

public abstract class ModulePatch
{
    /// <summary>
    ///     Method this patch targets
    /// </summary>
    public MethodBase? TargetMethod { get; private set; }

    /// <summary>
    ///     Is this patch active?
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Is this patch managed by the PatchManager?
    /// </summary>
    public bool IsManaged { get; private set; }

    /// <summary>
    ///     The harmony Id assigned to this patch, usually the name of the patch class.
    /// </summary>
    public string HarmonyId
    {
        get { return _harmony?.Id ?? "Harmony Id is null for this patch"; }
    }

    private Harmony? _harmony;

    private readonly List<HarmonyMethod> _prefixList;
    private readonly List<HarmonyMethod> _postfixList;
    private readonly List<HarmonyMethod> _transpilerList;
    private readonly List<HarmonyMethod> _finalizerList;
    private readonly List<HarmonyMethod> _ilmanipulatorList;

    protected static ManualLogSource Logger { get; private set; }

    protected ModulePatch()
        : this(null)
    {
        Logger ??= BepInEx.Logging.Logger.CreateLogSource(nameof(ModulePatch));
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">Name</param>
    protected ModulePatch(string name = null)
    {
        _harmony = new Harmony(name ?? GetType().Name);
        _prefixList = GetPatchMethods(typeof(PatchPrefixAttribute));
        _postfixList = GetPatchMethods(typeof(PatchPostfixAttribute));
        _transpilerList = GetPatchMethods(typeof(PatchTranspilerAttribute));
        _finalizerList = GetPatchMethods(typeof(PatchFinalizerAttribute));
        _ilmanipulatorList = GetPatchMethods(typeof(PatchILManipulatorAttribute));

        if (
            _prefixList.Count == 0
            && _postfixList.Count == 0
            && _transpilerList.Count == 0
            && _finalizerList.Count == 0
            && _ilmanipulatorList.Count == 0
        )
        {
            throw new PatchException($"{HarmonyId}: At least one of the patch methods must be specified");
        }
    }

    /// <summary>
    /// Get original method
    /// </summary>
    /// <returns>Method</returns>
    protected abstract MethodBase GetTargetMethod();

    /// <summary>
    /// Get HarmonyMethod from string
    /// </summary>
    /// <param name="attributeType">Attribute type</param>
    /// <returns>Method</returns>
    private List<HarmonyMethod> GetPatchMethods(Type attributeType)
    {
        var T = GetType();
        var methods = new List<HarmonyMethod>();

        foreach (var method in T.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            if (method.GetCustomAttribute(attributeType) != null)
            {
                methods.Add(new HarmonyMethod(method));
            }
        }

        return methods;
    }

    /// <summary>
    /// Apply patch to target
    /// </summary>
    public void Enable()
    {
        TargetMethod = GetTargetMethod();

        if (TargetMethod == null)
        {
            throw new PatchException($"{HarmonyId}: TargetMethod is null");
        }

        try
        {
            foreach (var prefix in _prefixList)
            {
                _harmony!.Patch(TargetMethod, prefix: prefix);
            }

            foreach (var postfix in _postfixList)
            {
                _harmony!.Patch(TargetMethod, postfix: postfix);
            }

            foreach (var transpiler in _transpilerList)
            {
                _harmony!.Patch(TargetMethod, transpiler: transpiler);
            }

            foreach (var finalizer in _finalizerList)
            {
                _harmony!.Patch(TargetMethod, finalizer: finalizer);
            }

            foreach (var ilmanipulator in _ilmanipulatorList)
            {
                _harmony!.Patch(TargetMethod, ilmanipulator: ilmanipulator);
            }

            Logger.LogInfo($"Enabled patch {HarmonyId}");

            ModPatchCache.AddPatch(this);
            IsActive = true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"{HarmonyId}: {ex}");
            throw new PatchException($"{HarmonyId}:", ex);
        }
    }

    /// <summary>
    ///     Internal use only, called from the patch manager.
    /// </summary>
    /// <param name="harmony">Harmony instance of the patch manager</param>
    internal void Enable(Harmony harmony)
    {
        if (!ReferenceEquals(_harmony, harmony))
        {
            // Override the initial harmony instance with the PatchManagers instance
            _harmony = harmony;
        }

        IsManaged = true;
        Enable();
    }

    /// <summary>
    /// Remove applied patch from target
    /// </summary>
    public void Disable()
    {
        TargetMethod = GetTargetMethod();

        if (TargetMethod == null)
        {
            throw new PatchException($"{_harmony.Id}: TargetMethod is null");
        }

        try
        {
            _harmony.Unpatch(TargetMethod, HarmonyPatchType.All, _harmony.Id);
            Logger.LogInfo($"Disabled patch {_harmony.Id}");

            ModPatchCache.RemovePatch(this);
            IsActive = false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"{_harmony.Id}: {ex}");
            throw new PatchException($"{_harmony.Id}:", ex);
        }
    }

    /// <summary>
    ///     Internal use only, called from the patch manager.
    /// </summary>
    /// <param name="harmony">Harmony instance of the patch manager</param>
    internal void Disable(Harmony harmony)
    {
        //  Attempting to disable a patch that is not managed by the patch manager
        if (harmony is null || !ReferenceEquals(_harmony, harmony))
        {
            throw new PatchException(
                $"Patch: {GetType().Name} is attempting to be disabled internally while not managed by the patch manager."
            );
        }

        Disable();

        // This patch is no longer considered managed.
        IsManaged = false;
    }
}
