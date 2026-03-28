using ML_KEM.Enums
using ML_KEM.Interfaces;
using ML_KEM.Model;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

public class PostQuantumCryptoService : IPostQuantumCryptoService
{
    private static readonly SecureRandom Rng = new SecureRandom();

    private static MLKemParameters GetParameters(MlKemLevel level) => level switch
    {
        MlKemLevel.MLKem512 => MLKemParameters.ml_kem_512,
        MlKemLevel.MLKem768 => MLKemParameters.ml_kem_768,
        MlKemLevel.MLKem1024 => MLKemParameters.ml_kem_1024,
        _ => throw new ArgumentException($"Niveli i pavlefshëm: {level}")
    };

    private static string GetAlgorithmName(MlKemLevel level) => level switch
    {
        MlKemLevel.MLKem512 => "ML-KEM-512",
        MlKemLevel.MLKem768 => "ML-KEM-768",
        MlKemLevel.MLKem1024 => "ML-KEM-1024",
        _ => "ML-KEM-768"
    };

    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair(MlKemLevel level)
    {
        var parameters = GetParameters(level);
        var gen = new MLKemKeyPairGenerator();
        gen.Init(new MLKemKeyGenerationParameters(Rng, parameters));
        var kp = gen.GenerateKeyPair();

        var pub = ((MLKemPublicKeyParameters)kp.Public).GetEncoded();
        var priv = ((MLKemPrivateKeyParameters)kp.Private).GetEncoded();
        return (pub, priv);
    }

    public EncryptResponseModel Encrypt(EncryptRequestModel req, MlKemLevel level)
    {
        var parameters = GetParameters(level);
        var pub = MLKemPublicKeyParameters.FromEncoding(parameters,
                    Convert.FromBase64String(req.ReceiverPublicKeyBase64));

        // Encapsulo sekretin simetrik me ML-KEM
        var enc = new MLKemEncapsulator(parameters);
        enc.Init(pub);
        var kemCipher = new byte[enc.EncapsulationLength];
        var shared = new byte[enc.SecretLength];
        enc.Encapsulate(kemCipher, 0, kemCipher.Length, shared, 0, shared.Length);

        // Enkripto plaintext-in me AES-GCM duke përdorur "shared"
        var nonce = RandomNumberGenerator.GetBytes(12);
        var pt = Convert.FromBase64String(req.PlaintextBase64);
        var ct = new byte[pt.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(shared, 16))
            aes.Encrypt(nonce, pt, ct, tag);

        return new EncryptResponseModel
        {
            MLKemAlgorithm = GetAlgorithmName(level),
            ReceiverPublicKeyBase64 = req.ReceiverPublicKeyBase64,
            KemCiphertextBase64 = Convert.ToBase64String(kemCipher),
            NonceBase64 = Convert.ToBase64String(nonce),
            CiphertextBase64 = Convert.ToBase64String(ct),
            TagBase64 = Convert.ToBase64String(tag),
            PlaintextLength = pt.Length
        };
    }

    public string Decrypt(DecryptRequestModel req, MlKemLevel level)
    {
        var parameters = GetParameters(level);
        var priv = MLKemPrivateKeyParameters.FromEncoding(parameters,
            Convert.FromBase64String(req.ReceiverPrivateKeyBase64));

        // Rikthe sekretin me ML-KEM
        var dec = new MLKemDecapsulator(parameters);
        dec.Init(priv);
        var shared = new byte[dec.SecretLength];
        var kemCipher = Convert.FromBase64String(req.KemCiphertextBase64);
        dec.Decapsulate(kemCipher, 0, kemCipher.Length, shared, 0, shared.Length);

        // Dekripto me AES-GCM
        var nonce = Convert.FromBase64String(req.NonceBase64);
        var ct = Convert.FromBase64String(req.CiphertextBase64);
        var tag = Convert.FromBase64String(req.TagBase64);
        var pt = new byte[ct.Length];
        using (var aes = new AesGcm(shared, 16))
            aes.Decrypt(nonce, ct, tag, pt);

        return Encoding.UTF8.GetString(pt);
    }
}
