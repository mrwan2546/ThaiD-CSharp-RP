using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using ThaIDAuthenAPIExample.Models;
using ThaIDAuthenAPIExample.Services;

namespace ThaIDAuthenAPIExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenInspectController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        private readonly ILogger<TokenInspectController> _logger;

        public TokenInspectController(ILogger<TokenInspectController> logger, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }


        [HttpGet(Name = "TokenInspect")]
        public async Task<TokenInspect> Get()
        {
            return await _authenticationService.TokenIntroSpectAsync(Request.Headers.Authorization);
        }


    }
}
