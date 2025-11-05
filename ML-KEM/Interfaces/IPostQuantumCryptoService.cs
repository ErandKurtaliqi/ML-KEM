using ML_KEM.Model;

namespace ML_KEM.Interfaces
{
    public interface IPostQuantumCryptoService
    {
        (byte[] publicKey, byte[] privateKey) GenerateKeyPair();
        EncryptResponseModel Encrypt(EncryptRequestModel req);
        string Decrypt(DecryptRequestModel req);
    }
}
