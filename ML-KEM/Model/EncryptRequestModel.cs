namespace ML_KEM.Model
{
    public sealed class EncryptRequestModel
    {
        public string ReceiverPublicKeyBase64 { get; set; } = default!;
        public string PlaintextBase64 { get; set; } = default!;
    }
}
