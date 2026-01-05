using Microsoft.AspNetCore.Mvc;
using ML_KEM.Enums;
using ML_KEM.Interfaces;
using ML_KEM.Model;
using System.Text;

namespace ML_KEM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostQuantumCryptoController : ControllerBase
    {
        private readonly IPostQuantumCryptoService _svc;
        public PostQuantumCryptoController(IPostQuantumCryptoService svc) => _svc = svc;

        /// <summary>
        /// Gjeneron çiftin e çelësave publik/privat për ML-KEM
        /// </summary>
        /// <param name="level">Niveli i sigurisë: 512, 768, ose 1024</param>
        [HttpGet("keys/{level}")]
        public IActionResult GenerateKeys(MlKemLevel level)
        {
            try
            {
                var (pub, priv) = _svc.GenerateKeyPair(level);
                return Ok(new KeyPairResponse
                {
                    PublicKeyBase64 = Convert.ToBase64String(pub),
                    PrivateKeyBase64 = Convert.ToBase64String(priv),
                    Algorithm = $"ML-KEM-{(int)level}",
                    PublicKeySize = pub.Length,
                    PrivateKeySize = priv.Length
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Enkripton tekstin me ML-KEM dhe AES-GCM
        /// </summary>
        [HttpPost("encrypt")]
        public IActionResult EncryptText([FromBody] EncryptTextRequest req)
        {
            try
            {
                var enc = _svc.Encrypt(new EncryptRequestModel
                {
                    ReceiverPublicKeyBase64 = req.ReceiverPublicKeyBase64,
                    PlaintextBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(req.Text))
                }, req.Level);
                return Ok(enc);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Dekripton mesazhin e enkriptuar
        /// </summary>
        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] DecryptRequestModel model)
        {
            try
            {
                var decrypted = _svc.Decrypt(model, model.Level);
                return Ok(new DecryptResponse
                {
                    DecryptedText = decrypted,
                    Algorithm = $"ML-KEM-{(int)model.Level}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Merr informacionet rreth algoritmeve të disponueshme
        /// </summary>
        [HttpGet("algorithms")]
        public IActionResult GetAlgorithms()
        {
            return Ok(new[]
            {
                new AlgorithmInfo
                {
                    Name = "ML-KEM-512",
                    Level = MlKemLevel.MLKem512,
                    SecurityLevel = "NIST Level 1 (128-bit)",
                    Description = "Nivel bazik sigurie, performancë më e lartë",
                    PublicKeySize = 800,
                    PrivateKeySize = 1632,
                    CiphertextSize = 768
                },
                new AlgorithmInfo
                {
                    Name = "ML-KEM-768",
                    Level = MlKemLevel.MLKem768,
                    SecurityLevel = "NIST Level 3 (192-bit)",
                    Description = "Nivel mesatar sigurie, balancë e mirë",
                    PublicKeySize = 1184,
                    PrivateKeySize = 2400,
                    CiphertextSize = 1088
                },
                new AlgorithmInfo
                {
                    Name = "ML-KEM-1024",
                    Level = MlKemLevel.MLKem1024,
                    SecurityLevel = "NIST Level 5 (256-bit)",
                    Description = "Nivel maksimal sigurie, për të dhëna kritike",
                    PublicKeySize = 1568,
                    PrivateKeySize = 3168,
                    CiphertextSize = 1568
                }
            });
        }
    }

    // Response models
    public class KeyPairResponse
    {
        public string PublicKeyBase64 { get; set; } = default!;
        public string PrivateKeyBase64 { get; set; } = default!;
        public string Algorithm { get; set; } = default!;
        public int PublicKeySize { get; set; }
        public int PrivateKeySize { get; set; }
    }

    public class DecryptResponse
    {
        public string DecryptedText { get; set; } = default!;
        public string Algorithm { get; set; } = default!;
    }

    public class AlgorithmInfo
    {
        public string Name { get; set; } = default!;
        public MlKemLevel Level { get; set; }
        public string SecurityLevel { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int PublicKeySize { get; set; }
        public int PrivateKeySize { get; set; }
        public int CiphertextSize { get; set; }
    }
}
