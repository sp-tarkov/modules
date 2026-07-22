using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;

namespace SPT.PrePatch;

public static class EnumPatcher
{
    public static void PatchEnums(ManualLogSource logger, ref AssemblyDefinition assembly, IReadOnlyCollection<EnumEntryDefinition> entries)
    {
        if (entries is null || entries.Count == 0)
        {
            return;
        }

        logger.LogInfo($"Patching {entries.Count} enum entries");

        foreach (var enumGroup in entries.GroupBy(entry => entry.EnumType))
        {
            var enumType = assembly.MainModule.GetType(enumGroup.Key);
            if (enumType is null || !enumType.IsEnum)
            {
                throw new InvalidOperationException($"Could not find enum type '{enumGroup.Key}' in Assembly-CSharp.dll.");
            }

            foreach (var entry in enumGroup)
            {
                if (string.IsNullOrWhiteSpace(entry.ConstantName))
                {
                    throw new InvalidOperationException($"The server returned an enum entry with no name for '{enumGroup.Key}'.");
                }

                if (enumType.Fields.Any(field => field.Name == entry.ConstantName))
                {
                    throw new InvalidOperationException($"Enum '{enumGroup.Key}' already contains an entry named '{entry.ConstantName}'.");
                }

                if (enumType.Fields.Any(field => field.HasConstant && Convert.ToInt64(field.Constant) == entry.ConstantValue))
                {
                    throw new InvalidOperationException($"Enum '{enumGroup.Key}' already contains the value {entry.ConstantValue}.");
                }

                enumType.Fields.Add(
                    CreateNewConstant(ref assembly, enumType, entry.JsonEnumName, entry.ConstantName, enumType, entry.ConstantValue)
                );
            }
        }

        logger.LogInfo($"Successfully patched {entries.Count} enum entries");
    }

    private static FieldDefinition CreateNewConstant(
        ref AssemblyDefinition assembly,
        TypeDefinition enumType,
        string attributeName,
        string enumName,
        TypeDefinition enumClass,
        long customConstant
    )
    {
        var jsonEnumNameAttribute = assembly.MainModule.GetType("EFT.JsonEnumNameAttribute");
        var ctor = jsonEnumNameAttribute.Methods.First(m => m.IsConstructor);
        var attribute = new CustomAttribute(ctor);

        var constant = new FieldDefinition(
            enumName,
            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
            enumClass
        )
        {
            Constant = ConvertConstant(enumType, customConstant),
        };

        if (!string.IsNullOrEmpty(attributeName))
        {
            var valueArgument = new CustomAttributeArgument(assembly.MainModule.TypeSystem.String, attributeName);
            attribute.ConstructorArguments.Add(valueArgument);
            constant.CustomAttributes.Add(attribute);
        }

        return constant;
    }

    private static object ConvertConstant(TypeDefinition enumType, long value)
    {
        var underlyingType = enumType.Fields.First(field => field.Name == "value__").FieldType.MetadataType;

        try
        {
            return underlyingType switch
            {
                MetadataType.SByte => checked((sbyte)value),
                MetadataType.Byte => checked((byte)value),
                MetadataType.Int16 => checked((short)value),
                MetadataType.UInt16 => checked((ushort)value),
                MetadataType.Int32 => checked((int)value),
                MetadataType.UInt32 => checked((uint)value),
                MetadataType.Int64 => value,
                MetadataType.UInt64 => checked((ulong)value),
                _ => throw new ModLoaderException($"Enum `{enumType.Name}` has an unsupported underlying type `{underlyingType}`."),
            };
        }
        catch (OverflowException exception)
        {
            throw new ModLoaderException(
                $"Value {value} does not fit enum `{enumType.Name}` underlying type `{underlyingType}`.",
                exception
            );
        }
    }
}
