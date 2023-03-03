using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.SinglePlayer.Patches.RaidFix
{
    public class RemoveUsedBotProfilePatch : ModulePatch
    {
        private static BindingFlags _flags;
        private static Type _targetInterface;
        private static Type _targetType;
        private static FieldInfo _profilesField;

        static RemoveUsedBotProfilePatch()
        {
            _ = nameof(IBotData.ChooseProfile);

            _flags = BindingFlags.Instance | BindingFlags.NonPublic;
            _targetInterface = PatchConstants.EftTypes.Single(IsTargetInterface);
            _targetType = PatchConstants.EftTypes.Single(IsTargetType);
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

        [PatchPrefix]
        private static bool PatchPrefix(ref Profile __result, object __instance, IBotData data)
        {
            var profiles = (List<Profile>)_profilesField.GetValue(__instance);

            if (profiles.Count > 0)
            {
                // second parameter makes client remove used profiles
                __result = data.ChooseProfile(profiles, true);
            }
            else
            {
                __result = null;
            }

            return false;
        }
    }
}
