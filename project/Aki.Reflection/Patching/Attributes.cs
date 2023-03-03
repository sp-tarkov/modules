using System;
using JetBrains.Annotations;

namespace Aki.Reflection.Patching
{
    [MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public class PatchPrefixAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
	public class PatchPostfixAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
	public class PatchTranspilerAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
	public class PatchFinalizerAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
	public class PatchILManipulatorAttribute : Attribute
    {
    }
}
