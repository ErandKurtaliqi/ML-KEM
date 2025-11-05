namespace ML_KEM.Model
{
    public sealed class EncryptResponseModel
    {
        public string MLKemAlgorithm { get; set; } = "Kyber-768";
        public string ReceiverPublicKeyBase64 { get; set; } = default!;
        public string KemCiphertextBase64 { get; set; } = default!;
        public string NonceBase64 { get; set; } = default!;
        public string CiphertextBase64 { get; set; } = default!;
        public string TagBase64 { get; set; } = default!;
        public int PlaintextLength { get; set; }
    }
}
