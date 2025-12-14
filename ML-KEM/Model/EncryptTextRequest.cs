using ML_KEM.Enums;

namespace ML_KEM.Model
{
    public class EncryptTextRequest
    {
        public string Text { get; set; } = default!;
        public string ReceiverPublicKeyBase64 { get; set; } = default!;
        public MlKemLevel Level { get; set; } = MlKemLevel.MLKem768;
    }
}
