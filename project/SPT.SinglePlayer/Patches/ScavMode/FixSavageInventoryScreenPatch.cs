using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Linq;


namespace SPT.SinglePlayer.Patches.ScavMode
{
    /// <summary>
    /// Get Profile at LocalGame End to use in FixSavageInventoryScreenPatch
    /// </summary>
    public class GetProfileAtEndOfRaidPatch : ModulePatch
    {
        public static string Profile { get; private set; }
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop));
        }

        [PatchPrefix]
        private static void PatchPrefix(LocalGame __instance)
        {
            GetProfileAtEndOfRaidPatch.Profile = __instance.Profile_0.ToJson();
        }
    }
    /// <summary>
    /// Get profile from other patch (GetProfileAtEndOfRaidPatch)
    /// if our profile is savage Create new Session.AllProfiles and pass in our own profile to allow us to use the ScavengerInventoryScreen
    /// </summary>
    public class FixSavageInventoryScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PostRaidHealthScreenClass), nameof(PostRaidHealthScreenClass.method_2));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref ISession ___iSession)
        {
            var profile = GetProfileAtEndOfRaidPatch.Profile.ParseJsonTo<Profile>();
            if (profile.Side != EPlayerSide.Savage)
            {
                return;
            }
            var session = (ProfileEndpointFactoryAbstractClass)___iSession;
            session.AllProfiles = new Profile[]
            {
                session.AllProfiles.First(x => x.Side != EPlayerSide.Savage),
                profile
            };
            session.ProfileOfPet.UncoverAll();
        }
    }
}
