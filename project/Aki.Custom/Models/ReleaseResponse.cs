namespace Aki.Custom.Models
{
    public struct ReleaseResponse
    {
        public bool isBeta { get; set; }
        public string betaDisclaimer { get; set; }
        public float betaDisclaimerTimeoutDelay { get; set; }
        public string releaseSummary { get; set; }
        
    }
}