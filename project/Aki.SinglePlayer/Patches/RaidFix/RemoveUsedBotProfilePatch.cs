using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class RemoveUsedBotProfilePatch : ModulePatch
    {
        private static readonly BindingFlags _flags;
        private static readonly Type _targetInterface;
        private static readonly Type _targetType;
        private static readonly FieldInfo _profilesField;

        static RemoveUsedBotProfilePatch()
        {
            _ = nameof(IGetProfileData.ChooseProfile);

            _flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _targetInterface = PatchConstants.EftTypes.Single(IsTargetInterface);
            _targetType = typeof(BotsPresets);
            _profilesField = _targetType.GetField("list_0", _flags);
        }

        protected override MethodBase GetTargetMethod()
        {
            return _targetType.GetMethod("GetNewProfile", _flags);
        }

        private static bool IsTargetInterface(Type type)
        {
            return type.IsInterface && type.GetProperty("StartProfilesLoaded") != null && type.GetMethod("CreateProfile") != null;
        }

        private static bool IsTargetType(Type type)
        {
            return _targetInterface.IsAssignableFrom(type) && _targetInterface.IsAssignableFrom(type.BaseType);
        }

        /// <summary>
        /// BotsPresets.GetNewProfile()
        [PatchPrefix]
        private static bool PatchPrefix(ref Profile __result, object __instance, GClass560 data, ref bool withDelete)
        {
            withDelete = true;

            return true; // Do original method
        }
    }
}
