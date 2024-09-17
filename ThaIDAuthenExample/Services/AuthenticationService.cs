using IdentityModel.OidcClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using ThaIDAuthenExample.Models;

namespace ThaIDAuthenExample.Services
{
    public interface IAuthenticationService
    {
        public Task<AuthorizeState> CreateProvider();
        public Task<TokenResponse> RequestTokenAsync(string code, string state);
        public Task<TokenResponse> RefreshTokenAsync(TokenResponse tokenOriginal);
        public Task<bool> ValidateIdToken(string keyForValidate, string idToken);
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly OidcClient _provider;
        public AuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            var config = new OidcClientOptions
            {
                Authority = _configuration["ThaID:Issuer"],
                ClientId = _configuration["ThaID:ClientID"],
                ClientSecret = _configuration["ThaID:ClientSecret"],
                RedirectUri = _configuration["ThaID:RedirectURL"],
                Scope = _configuration["ThaID:Scope"]
            };
            _provider = new OidcClient(config);
        }
        public async Task<AuthorizeState> CreateProvider()
        {
            var configProvider = _provider.PrepareLoginAsync().Result;
            return configProvider;
        }
        public async Task<TokenResponse> RequestTokenAsync(string code, string state)
        {
            var configToken = _provider.PrepareLoginAsync().Result;
            configToken.StartUrl = $"{_configuration["ThaID:RedirectURL"]}?code={code}&state={state}";
            configToken.State = state;

            // Get token
            var resultToken = await _provider.ProcessResponseAsync(configToken.StartUrl, configToken);

            // Convert to TokenResponse model
            TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(resultToken.TokenResponse.Raw);
            return tokenResponse;
        }
        public async Task<TokenResponse> RefreshTokenAsync(TokenResponse tokenOriginal)
        {
            // refresh token
            var resultToken = await _provider.RefreshTokenAsync(tokenOriginal.RefreshToken);
            // convert to model for use
            TokenResponse tokenResponse = new TokenResponse()
            {
                AccessToken = resultToken.AccessToken,
                RefreshToken = resultToken.RefreshToken,
                TokenType = tokenOriginal.TokenType,
                ExpiresIn = resultToken.ExpiresIn,
                Scope = tokenOriginal.Scope,
                IDToken = resultToken.IdentityToken
            };
            return tokenResponse;
        }
        public async Task<bool> ValidateIdToken(string keyForValidate, string idToken)
        {
            // ValidateIdToken
            try
            {
                var httpClient = new HttpClient();
                var jwksResponse = await httpClient.GetAsync(_configuration["ThaID:URLJwks"]);
                var jwks = await jwksResponse.Content.ReadAsStringAsync();
                var keySet = new JsonWebKeySet(jwks);
                var publicKey = keySet.Keys.Where(value => value.KeyId == keyForValidate).FirstOrDefault();
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _provider.Options.Authority,
                    ValidAudience = _provider.Options.ClientId,
                    IssuerSigningKey = publicKey // Set the public key here
                };
                var principal = tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);

                if (principal.Identity.IsAuthenticated)
                {
                    return true;
                }
                else
                {
                    return false;
                }

                

            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
