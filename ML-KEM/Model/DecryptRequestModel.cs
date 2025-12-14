using ML_KEM.Enums;

namespace ML_KEM.Model
{
    public class DecryptRequestModel
    {
        public string ReceiverPrivateKeyBase64 { get; set; } = default!;
        public string KemCiphertextBase64 { get; set; } = default!;
        public string NonceBase64 { get; set; } = default!;
        public string CiphertextBase64 { get; set; } = default!;
        public string TagBase64 { get; set; } = default!;
        public MlKemLevel Level { get; set; } = MlKemLevel.MLKem768;
    }
}
