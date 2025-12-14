using ML_KEM.Enums;
using ML_KEM.Model;

namespace ML_KEM.Interfaces
{
    public interface IPostQuantumCryptoService
    {
        (byte[] publicKey, byte[] privateKey) GenerateKeyPair(MlKemLevel level);
        EncryptResponseModel Encrypt(EncryptRequestModel req, MlKemLevel level);
        string Decrypt(DecryptRequestModel req, MlKemLevel level);
    }
}
