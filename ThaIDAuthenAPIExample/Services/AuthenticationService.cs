using Microsoft.Net.Http.Headers;
using System.Text.Json;
using ThaIDAuthenAPIExample.Models;

namespace ThaIDAuthenAPIExample.Services
{
    public interface IAuthenticationService
    {
        public Task<TokenInspect> TokenIntroSpectAsync(string token);
        public Task<TokenRevoke> TokenRevokeAsync(string token);
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public AuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<TokenInspect> TokenIntroSpectAsync(string token)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("DOPA");


            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Authorization,
                $"Basic {ClientAuthen(_configuration["ThaID:ClientID"], _configuration["ThaID:ClientSecret"])}"
            );

            Dictionary<string, string> requestToken = new Dictionary<string, string>
            {
                { "token", token }
            };

            HttpContent httpContent = new FormUrlEncodedContent(requestToken);

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("api/v2/oauth2/introspect/", httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            string responseStr = await httpResponseMessage.Content.ReadAsStringAsync();
            TokenInspect tokenResponse = JsonSerializer.Deserialize<TokenInspect>(responseStr);
            return tokenResponse;
        }

        public async Task<TokenRevoke> TokenRevokeAsync(string token)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("DOPA");


            httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Authorization,
                $"Basic {ClientAuthen(_configuration["ThaID:ClientID"], _configuration["ThaID:ClientSecret"])}"
            );

            Dictionary<string, string> requestToken = new Dictionary<string, string>
            {
                { "token", token }
            };

            HttpContent httpContent = new FormUrlEncodedContent(requestToken);

            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("api/v2/oauth2/revoke/", httpContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            string responseStr = await httpResponseMessage.Content.ReadAsStringAsync();
            TokenRevoke tokenResponse = JsonSerializer.Deserialize<TokenRevoke>(responseStr);
            return tokenResponse;
        }

        private string ClientAuthen(string clientID, string clientSecret)
        {
            byte[] clientAuthen = System.Text.Encoding.UTF8.GetBytes($"{clientID}:{clientSecret}");
            return Convert.ToBase64String(clientAuthen);
        }
    }
}
