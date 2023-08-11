using Aki.Reflection.Patching;
using EFT;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// Patch account ID to a hardcoded integer-parseable string when creating MongoIDs to prevent string -> uint32 casting issues.
    /// Has no effect on game functionality besides possibly generating a more deterministic ID.
    /// </summary>
    public class MongoIDPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Only affects the MongoID constructor that takes a Profile parameter.
            return typeof(MongoID).GetConstructor(new System.Type[1] { typeof(Profile) });
        }

        [PatchPrefix]
        private static bool PatchPrefix(Profile profile)
        {
            profile.AccountId = "1024";
            return true;
        }
    }
}
