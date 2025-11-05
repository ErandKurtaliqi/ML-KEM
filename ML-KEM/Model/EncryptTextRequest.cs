namespace ML_KEM.Model
{
    public class EncryptTextRequest
    {
        public string Text { get; set; } = default!;
        public string ReceiverPublicKeyBase64 { get; set; } = default!;
    }
}
