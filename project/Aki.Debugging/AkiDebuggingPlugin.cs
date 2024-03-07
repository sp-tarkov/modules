using System;
using System.Runtime.CompilerServices;
using Aki.Common;
using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Custom.Models;
using Aki.Debugging.Patches;
using BepInEx;
using Comfort.Common;
using EFT;
using EFT.UI;
using UnityEngine;

namespace Aki.Debugging
{
    [BepInPlugin("com.spt-aki.debugging", "AKI.Debugging", AkiPluginInfo.PLUGIN_VERSION)]
    public class AkiDebuggingPlugin : BaseUnityPlugin
    {
        public static string sptVersion;
        private ReleaseResponse release;

        private bool _isBetaDisclaimerOpen = false;

        public void Awake()
        {
            Logger.LogInfo("Loading: Aki.Debugging");

            try
            {
                new EndRaidDebug().Enable();
                // new CoordinatesPatch().Enable();
                // new StaticLootDumper().Enable();

                // BTR debug command patches, can be disabled later
                //new BTRDebugCommandPatch().Enable();
                //new BTRDebugDataPatch().Enable();

                //new PMCBotSpawnLocationPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }

            Logger.LogInfo("Completed: Aki.Debugging");
        }

        public void Start()
        {
            var versionJson = RequestHandler.GetJson("/singleplayer/settings/version");
            sptVersion = Json.Deserialize<VersionResponse>(versionJson).Version;

            var releaseJson = RequestHandler.GetJson("/singleplayer/release");
            release = Json.Deserialize<ReleaseResponse>(releaseJson);

            SetVersionPref();

            // Enable the watermark if this is a bleeding edge build.
            // Enabled in start to allow time for the request containing the bool to process.
            if (release.isBeta)
            {
                new DebugLogoPatch().Enable();
                new DebugLogoPatch2().Enable();
                new DebugLogoPatch3().Enable();
            }

            if (release.isBeta && PlayerPrefs.GetInt("SPT_AcceptedBETerms") == 1)
            {
                Logger.LogInfo("User accepted the beta disclaimer");
            }
        }

        public void Update()
        {
            if (Singleton<PreloaderUI>.Instantiated && ShouldShowBetaMessage())
            {
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(sptVersion, release.betaDisclaimer, ErrorScreen.EButtonType.OkButton, release.betaDisclaimerTimeoutDelay, new Action(OnMessageAccepted), new Action(OnTimeOut));
                _isBetaDisclaimerOpen = true;
            }

            if (Singleton<PreloaderUI>.Instantiated && ShouldShowReleaseNotes())
            {
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(sptVersion, release.releaseSummary, ErrorScreen.EButtonType.OkButton, 36000, null, null);
                PlayerPrefs.SetInt("SPT_ShownReleaseNotes", 1);
            }
        }

        // User accepted the terms, allow to continue.
        private void OnMessageAccepted()
        {
            Logger.LogInfo("User accepted the terms");
            PlayerPrefs.SetInt("SPT_AcceptedBETerms", 1);
            _isBetaDisclaimerOpen = false;
        }

        // If the user doesnt accept the message "Ok" then the game will close.
        private void OnTimeOut()
        {
            Application.Quit();
        }

        // Stores the current build in the registry to check later
        // Return true if changed, false if not
        private void SetVersionPref()
        {
            if (GetVersionPref() == string.Empty || GetVersionPref() != sptVersion)
            {
                PlayerPrefs.SetString("SPT_Version", sptVersion);

                // 0 val used to indicate false, 1 val used to indicate true
                PlayerPrefs.SetInt("SPT_AcceptedBETerms", 0);
                PlayerPrefs.SetInt("SPT_ShownReleaseNotes", 0);
            }
        }

        // Retrieves the current build from the registry to check against the current build
        // If this is the first run and no entry exists returns an empty string
        private string GetVersionPref()
        {
            return PlayerPrefs.GetString("SPT_Version", string.Empty);
        }

        // Should we show the message, only show if first run or if build has changed
        private bool ShouldShowBetaMessage()
        {
            return PlayerPrefs.GetInt("SPT_AcceptedBETerms") == 0 && release.isBeta  && !_isBetaDisclaimerOpen ? true : false;
        }

        // Should we show the release notes, only show on first run or if build has changed
        private bool ShouldShowReleaseNotes()
        {
            return PlayerPrefs.GetInt("SPT_ShownReleaseNotes") == 0  && !_isBetaDisclaimerOpen ? true : false;
        }
    }
}
