namespace SPT.Custom.Models;

public struct ReleaseResponse
{
    public bool isBeta { get; set; }
    public bool isModdable { get; set; }
    public bool isModded { get; set; }
    public float betaDisclaimerTimeoutDelay { get; set; }
    public string betaDisclaimerText { get; set; }
    public string betaDisclaimerAcceptText { get; set; }
    public string serverModsLoadedText { get; set; }
    public string serverModsLoadedDebugText { get; set; }
    public string clientModsLoadedText { get; set; }
    public string clientModsLoadedDebugText { get; set; }
    public string illegalPluginsLoadedText { get; set; }
    public string illegalPluginsExceptionText { get; set; }
    public string releaseSummaryText { get; set; }
}
