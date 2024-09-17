using Microsoft.AspNetCore.Mvc;
using ThaIDAuthenAPIExample.Models;
using ThaIDAuthenAPIExample.Services;

namespace ThaIDAuthenAPIExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenRevokeController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        private readonly ILogger<TokenInspectController> _logger;

        public TokenRevokeController(ILogger<TokenInspectController> logger, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }

        [HttpGet(Name = "TokenRevoke")]
        public async Task<TokenRevoke> Get()
        {
            return await _authenticationService.TokenRevokeAsync(Request.Headers.Authorization);
        }
    }
}
