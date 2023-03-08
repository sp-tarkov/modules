using System;
using Aki.Custom.Airdrops.Patches;
using Aki.Custom.Patches;
using Aki.Custom.Utils;
using BepInEx;

namespace Aki.Custom
{
    [BepInPlugin("com.spt-aki.custom", "AKI.Custom", "1.0.0")]
    class AkiCustomPlugin : BaseUnityPlugin
    {
        public AkiCustomPlugin()
        {
            Logger.LogInfo("Loading: Aki.Custom");

            try
            {
                // Bundle patches should always load first
                BundleManager.GetBundles();
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();

                new BossSpawnChancePatch().Enable();
                new BotDifficultyPatch().Enable();
                new CoreDifficultyPatch().Enable();
                new OfflineRaidMenuPatch().Enable();
                new OfflineRaidSettingsMenuPatch().Enable();
                new SessionIdPatch().Enable();
                new VersionLabelPatch().Enable();
                new IsEnemyPatch().Enable();
                //new AddSelfAsEnemyPatch().Enable();
                new CheckAndAddEnemyPatch().Enable();
                new BotSelfEnemyPatch().Enable(); // needed
                new AddEnemyToAllGroupsInBotZonePatch().Enable();
                new AirdropPatch().Enable();
                new AirdropFlarePatch().Enable();
                new AddSptBotSettingsPatch().Enable();
                new CustomAiPatch().Enable();
                new ExitWhileLootingPatch().Enable();
                new QTEPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.Custom");
        }
    }
}
