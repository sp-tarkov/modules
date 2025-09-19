using UnityEngine.Networking;

namespace SPT.Common.Models;

public class FakeCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}
