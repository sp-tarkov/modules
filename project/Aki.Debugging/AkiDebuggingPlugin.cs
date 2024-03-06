using System;
using Aki.Common;
using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Custom.Models;
using Aki.Debugging.Patches;
using BepInEx;
using Comfort.Common;
using EFT.UI;
using UnityEngine;

namespace Aki.Debugging
{
    [BepInPlugin("com.spt-aki.debugging", "AKI.Debugging", AkiPluginInfo.PLUGIN_VERSION)]
    public class AkiDebuggingPlugin : BaseUnityPlugin
    {

        public static string sptVersion;

        // Disable this in release builds.
        private bool _isBleeding = true;
        private readonly string _message = "By pressing OK you agree that no support is offered and that this is for bug testing only. NOT actual gameplay."
        + " Mods are disabled. New profiles may be required frequently. Report all bugs in the reports channel in discord, or on the issues page on the website."
        + " If you don't press OK by the time specified, the game will close.";

        // How long before the message times out and closes the game.
        private const float _timeOutDelay = 30f;

        // Is this a new build?
        private bool _hasVersionChangedSinceLastRun = false;

        // Is the message open, avoids reinstantiating the error screen over and over again.
        private bool _IsMessageOpen = false;

        public void Awake()
        {
            Logger.LogInfo("Loading: Aki.Debugging");

            try
            {
                new EndRaidDebug().Enable();
                // new CoordinatesPatch().Enable();
                // new StaticLootDumper().Enable();
                
                // Enable the watermark if this is a bleeding edge build.
                if (_isBleeding)
                {
                    new DebugLogoPatch().Enable();
                    new DebugLogoPatch2().Enable();
                    new DebugLogoPatch3().Enable();
                }
                
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
            var json = RequestHandler.GetJson("/singleplayer/settings/version");
            sptVersion = Json.Deserialize<VersionResponse>(json).Version;

            _hasVersionChangedSinceLastRun = SetVersionPref();
        }

        public void Update()
        {
            if (Singleton<PreloaderUI>.Instantiated && ShouldShowMessage() && !_IsMessageOpen && _isBleeding)
            {
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(sptVersion, _message, ErrorScreen.EButtonType.OkButton, _timeOutDelay, new Action(OnMessageAccepted), new Action(OnTimeOut));
                _IsMessageOpen = true;
            }
        }

        // User accepted the terms, allow to continue.
        private void OnMessageAccepted()
        {
            Logger.LogInfo("User accepted the terms");
        }

        // If the user doesnt accept the message "Ok" then the game will close.
        private void OnTimeOut()
        {
            Application.Quit();
        }

        // Stores the current build in the registry to check later
        // Return true if changed, false if not
        private bool SetVersionPref()
        {
            if (GetVersionPref() == string.Empty || GetVersionPref() != sptVersion)
            {
                PlayerPrefs.SetString("SPT_Version", sptVersion);
                return true;
            }

            return false;
        }

        // Retrieves the current build from the registry to check against the current build
        // If this is the first run and no entry exists, returns an empty string
        private string GetVersionPref()
        {
            return PlayerPrefs.GetString("SPT_Version", string.Empty);
        }

        // Should we show the message, only show if first run
        // or if build has changed.
        private bool ShouldShowMessage()
        {
            return GetVersionPref() != sptVersion || _hasVersionChangedSinceLastRun ? true : false;
        }
    }
}
