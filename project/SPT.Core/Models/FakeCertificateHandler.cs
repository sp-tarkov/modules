using UnityEngine.Networking;

namespace SPT.Core.Models;

public class FakeCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}
