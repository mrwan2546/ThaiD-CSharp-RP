using IdentityModel.OidcClient;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json;
using ThaIDAuthenExample.Models;
using ThaIDAuthenExample.Services;

namespace ThaIDAuthenExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;

        public HomeController(ILogger<HomeController> logger, IConfiguration config, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _configuration = config;
            _authenticationService = authenticationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/authentication/login")]
        public async Task<IActionResult> login()
        {
            AuthorizeState provider = await _authenticationService.CreateProvider();
            return Redirect(provider.StartUrl);
        }

        [Route("/authentication/login-callback")]
        public async Task<IActionResult> Authentication(string code, string state)
        {
            TokenResponse tokenResponse = await _authenticationService.RequestTokenAsync(code, state);
            CreateSessionToken(tokenResponse);
            return View("Authentication",tokenResponse);
        }
        
        [Route("/authentication/RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var jsonSessionToken = HttpContext.Session.GetString("token");
            if (jsonSessionToken != null)
            {
                var tokenSession = JsonSerializer.Deserialize<TokenResponse>(jsonSessionToken);
                TokenResponse tokenResponse = await _authenticationService.RefreshTokenAsync(tokenSession);
                CreateSessionToken(tokenResponse);
                return View("Authentication", tokenResponse);
            }
            else
            {
                return View("Authentication");
            }
        }

        [Route("/authentication/validateIdToken")]
        public async Task<bool> ValidateIdToken()
        {
            var jsonSessionToken = HttpContext.Session.GetString("token");
            if (jsonSessionToken != null)
            {
                try
                {
                    var tokenSession = JsonSerializer.Deserialize<TokenResponse>(jsonSessionToken);
                    var resultValidate = await _authenticationService.ValidateIdToken(tokenSession.IDTokenJWT.Header.Kid, tokenSession.IDToken);
                    return resultValidate;
                }
                catch (Exception ex)
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public void CreateSessionToken(TokenResponse tokenForSet)
        {
            HttpContext.Session.SetString("token", JsonSerializer.Serialize(tokenForSet));
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
