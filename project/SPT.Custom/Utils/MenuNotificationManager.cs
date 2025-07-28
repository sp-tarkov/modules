using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT.UI;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Custom.Patches;
using UnityEngine;

namespace SPT.Custom.Utils;

public class MenuNotificationManager : MonoBehaviour
{
    private static bool _seenBetaMessage = false;
    public static string SptVersion;
    public static string CommitHash;
    public static string[] DisallowedPlugins;
    internal static HashSet<string> WhitelistedPlugins =
    [
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
        "RuntimeUnityEditor",
        "com.dirtbikercj.debugplus",
    ];
    internal static ReleaseResponse release;
    private bool _isBetaDisclaimerOpen;
    private ManualLogSource _logger;

    /// <summary>
    /// This GClass can be found by looking at <see cref="ErrorScreen"/> and seeing what the ErrorScreen class inherits from: <see cref="Window{T}"/> <br/>
    /// The constrained generic is the class to use
    /// </summary>
    private GClass3629 _betaMessageContext;

    /// <summary>
    /// Retrieves the current build from the registry to check against the current build <br/>
    /// If this is the first run and no entry exists returns an empty string
    /// </summary>
    private string VersionPref
    {
        get { return PlayerPrefs.GetString("SPT_Version", string.Empty); }
    }

    /// <summary>
    /// Should we show the message, only show if first run or if build has changed
    /// </summary>
    private bool ShouldShowBetaMessage
    {
        get
        {
            return PlayerPrefs.GetInt("SPT_AcceptedBETerms") == 0
                   && release.isBeta
                   && !_isBetaDisclaimerOpen;
        }
    }

    public void Start()
    {
        _logger = BepInEx.Logging.Logger.CreateLogSource(nameof(MenuNotificationManager));

        var versionJson = RequestHandler.GetJson("/singleplayer/settings/version");
        SptVersion = Json.Deserialize<VersionResponse>(versionJson).Version;
        CommitHash = SptVersion?.Trim()?.Split(' ')?.Last() ?? "";

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

        DisallowedPlugins = Chainloader
            .PluginInfos.Values.Select(pi => pi.Metadata.GUID)
            .Except(WhitelistedPlugins)
            .ToArray();

        // Prevent client mods if the server is built with mods disabled
        if (!release.isModdable)
        {
            new PreventClientModsPatch().Enable();
        }

        if (release.isBeta && PlayerPrefs.GetInt("SPT_AcceptedBETerms") == 1)
        {
            _logger.LogInfo(release.betaDisclaimerAcceptText);
            ServerLog.Info("SPT.Custom", release.betaDisclaimerAcceptText);
        }

        if (release.isModded && release.isBeta && release.isModdable)
        {
            CommitHash += $"\n {release.serverModsLoadedDebugText}";
            ServerLog.Warn("SPT.Custom", release.serverModsLoadedText);
        }

        if (DisallowedPlugins.Any() && release.isBeta && release.isModdable)
        {
            CommitHash += $"\n {release.clientModsLoadedDebugText}";
            ServerLog.Warn(
                "SPT.Custom",
                $"{release.clientModsLoadedText}\n{string.Join("\n", DisallowedPlugins)}"
            );
        }
    }

    public void Update()
    {
        if (SptVersion == null)
        {
            return;
        }

        if (_seenBetaMessage)
        {
            return;
        }

        if (!Singleton<PreloaderUI>.Instantiated)
        {
            return;
        }

        ShowBetaMessage();

        _seenBetaMessage = true;

        // This mono has served its purpose
        Destroy(this);
    }

    // Show the beta message
    // if mods are enabled show that mods are loaded in the message.
    private void ShowBetaMessage()
    {
        if (Singleton<PreloaderUI>.Instantiated && ShouldShowBetaMessage)
        {
            _betaMessageContext = Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(
                SptVersion,
                release.betaDisclaimerText,
                ErrorScreen.EButtonType.OkButton,
                release.betaDisclaimerTimeoutDelay
            );
            // Note: This looks backwards, but a timeout counts as "Accept" and clicking the button counts as "Decline"
            _betaMessageContext.OnAccept += OnBetaMessageTimeOut;
            _betaMessageContext.OnDecline += OnBetaMessageAccepted;
            _betaMessageContext.OnClose += OnBetaMessageClosed;
            _isBetaDisclaimerOpen = true;
        }
    }

    // User accepted the BE terms, allow to continue.
    private void OnBetaMessageAccepted()
    {
        PlayerPrefs.SetInt("SPT_AcceptedBETerms", 1);
        _isBetaDisclaimerOpen = false;
    }

    // User didn't accept the BE terms, exit the game
    private void OnBetaMessageTimeOut()
    {
        Application.Quit();
    }

    // Unhook events once the beta message is closed
    private void OnBetaMessageClosed()
    {
        // Note: This looks backwards, but a timeout counts as "Accept" and clicking the button counts as "Decline"
        _betaMessageContext.OnAccept -= OnBetaMessageTimeOut;
        _betaMessageContext.OnDecline -= OnBetaMessageAccepted;
        _betaMessageContext.OnClose -= OnBetaMessageClosed;
    }

    // Stores the current build in the registry to check later
    // Return true if changed, false if not
    private void SetVersionPref()
    {
        if (VersionPref == string.Empty || VersionPref != SptVersion)
        {
            PlayerPrefs.SetString("SPT_Version", SptVersion);

            // 0 val used to indicate false, 1 val used to indicate true
            PlayerPrefs.SetInt("SPT_AcceptedBETerms", 0);
        }
    }
}