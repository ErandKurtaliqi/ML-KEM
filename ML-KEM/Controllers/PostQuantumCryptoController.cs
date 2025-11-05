using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("keys")]
        public IActionResult GenerateKeys()
        {
            var (pub, priv) = _svc.GenerateKeyPair();
            return Ok(new
            {
                PublicKeyBase64 = Convert.ToBase64String(pub),
                PrivateKeyBase64 = Convert.ToBase64String(priv)
            });
        }

        [HttpPost("encrypt")]
        public IActionResult EncryptText([FromBody] EncryptTextRequest req)
        {
            var enc = _svc.Encrypt(new EncryptRequestModel
            {
                ReceiverPublicKeyBase64 = req.ReceiverPublicKeyBase64,
                PlaintextBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(req.Text))
            });
            return Ok(enc);
        }

        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] DecryptRequestModel model)
            => Ok(_svc.Decrypt(model));
    }
}