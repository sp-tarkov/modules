using System;
using JetBrains.Annotations;

namespace SPT.Reflection.Patching;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class PatchPrefixAttribute : Attribute { }

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class PatchPostfixAttribute : Attribute { }

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class PatchTranspilerAttribute : Attribute { }

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class PatchFinalizerAttribute : Attribute { }

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class PatchILManipulatorAttribute : Attribute { }

/// <summary>
///     If added to a patch, it will not be used during auto patching
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class IgnoreAutoPatchAttribute : Attribute;

/// <summary>
///     If added to a patch, it will only be enabled during debug builds
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DebugPatchAttribute : Attribute;
