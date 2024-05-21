using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.SinglePlayer.Patches.MainMenu;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPT.SinglePlayer.Utils.MainMenu
{
    public class MenuNotificationManager : MonoBehaviour
    {
        public static string sptVersion;
        public static string commitHash;
        internal static HashSet<string> whitelistedPlugins = new HashSet<string>
        {
                "com.SPT.core",
                "com.SPT.custom",
                "com.SPT.debugging",
                "com.SPT.singleplayer",
                "com.bepis.bepinex.configurationmanager",
                "com.terkoiz.freecam",
                "com.sinai.unityexplorer",
                "com.cwx.debuggingtool-dxyz",
                "com.cwx.debuggingtool",
                "xyz.drakia.botdebug",
                "com.kobrakon.camunsnap",
                "RuntimeUnityEditor"
        };
        
        public static string[] disallowedPlugins;
        internal static ReleaseResponse release;
        private bool _isBetaDisclaimerOpen = false;
        private ManualLogSource Logger;

        public void Start()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(MenuNotificationManager));

            var versionJson = RequestHandler.GetJson("/singleplayer/settings/version");
            sptVersion = Json.Deserialize<VersionResponse>(versionJson).Version;
            commitHash = sptVersion?.Trim()?.Split(' ')?.Last() ?? "";

            var releaseJson = RequestHandler.GetJson("/singleplayer/release");
            release = Json.Deserialize<ReleaseResponse>(releaseJson);

            SetVersionPref();

            // Enable the watermark if this is a bleeding edge build.
            // Enabled in start to allow time for the request containing the bool to process.
            if (release.isBeta)
            {
                new BetaLogoPatch().Enable();
                new BetaLogoPatch2().Enable();
                new BetaLogoPatch3().Enable();
            }

            disallowedPlugins = Chainloader.PluginInfos.Values
                .Select(pi => pi.Metadata.GUID).Except(whitelistedPlugins).ToArray();

            // Prevent client mods if the server is built with mods disabled
            if (!release.isModdable)
            {
                new PreventClientModsPatch().Enable();
            }
        
            if (release.isBeta && PlayerPrefs.GetInt("SPT_AcceptedBETerms") == 1)
            {
                Logger.LogInfo(release.betaDisclaimerAcceptText);
                ServerLog.Info("SPT.Custom", release.betaDisclaimerAcceptText);
            }

            if (release.isModded && release.isBeta && release.isModdable)
            {
                commitHash += $"\n {release.serverModsLoadedDebugText}";
                ServerLog.Warn("SPT.Custom", release.serverModsLoadedText);
            }

            if (disallowedPlugins.Any() && release.isBeta && release.isModdable)
            {
                commitHash += $"\n {release.clientModsLoadedDebugText}";
                ServerLog.Warn("SPT.Custom", $"{release.clientModsLoadedText}\n{string.Join("\n", disallowedPlugins)}");
            }
        }
        public void Update()
        {
            if (sptVersion == null)
            {
                return;
            }

            ShowBetaMessage();
            ShowReleaseNotes();
        }

        // Show the beta message
        // if mods are enabled show that mods are loaded in the message.
        private void ShowBetaMessage()
        {
            if (Singleton<PreloaderUI>.Instantiated && ShouldShowBetaMessage())
            {
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(sptVersion, release.betaDisclaimerText, ErrorScreen.EButtonType.OkButton, release.betaDisclaimerTimeoutDelay, new Action(OnMessageAccepted), new Action(OnTimeOut));
                _isBetaDisclaimerOpen = true;
            }
        }

        // Show the release notes.
        private void ShowReleaseNotes()
        {
            if (Singleton<PreloaderUI>.Instantiated && ShouldShowReleaseNotes())
            {
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(sptVersion, release.releaseSummaryText, ErrorScreen.EButtonType.OkButton, 36000, null, null);
                PlayerPrefs.SetInt("SPT_ShownReleaseNotes", 1);
            }
        }

        // User accepted the terms, allow to continue.
        private void OnMessageAccepted()
        {
            Logger.LogInfo(release.betaDisclaimerAcceptText);
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
            return PlayerPrefs.GetInt("SPT_AcceptedBETerms") == 0 && release.isBeta && !_isBetaDisclaimerOpen ? true : false;
        }

        // Should we show the release notes, only show on first run or if build has changed
        private bool ShouldShowReleaseNotes()
        {
            return PlayerPrefs.GetInt("SPT_ShownReleaseNotes") == 0 && !_isBetaDisclaimerOpen && release.releaseSummaryText != string.Empty ? true : false;
        }
    }
}
