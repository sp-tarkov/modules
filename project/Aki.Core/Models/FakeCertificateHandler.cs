using UnityEngine.Networking;
using Aki.Core.Utils;

namespace Aki.Core.Models
{
    public class FakeCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return ValidationUtil.Validate();
        }
    }
}
