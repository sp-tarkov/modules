using UnityEngine.Networking;
using SPT.Core.Utils;

namespace SPT.Core.Models
{
    public class FakeCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }
}
