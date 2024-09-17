# ThaID OAuth2 Integration with .NET
This project shows how to connect **ThaID** to the **.NET Framework** using **OAuth2 authentication**. It lets you safely log in and give users access through ThaID.

The solution contains two projects
- ThaIDAuthenExample: Used to test the connection to ThaID.
- ThaIDAuthenAPIExample: Simulates an authorized resource for cross-system API testing.
---
## 📁 ThaIDAuthenExample ##
## # Settings for connecting to ThaID ##
location: `ThaIDAuthenExample/appsettings.json`

Variables for **configuration** used with **ThaID** data integration, such as **client ID, client secret, APIKey, Callback URL and Scope**.
```json
{
  "ThaID": {
    "ClientID": "{Client_ID}",
    "ClientSecret": "{Client_Secret}",
    "APIKey": "{API_Key}",
    "RedirectURL": "{Callback_URL}",
    "Issuer": "https://imauth.bora.dopa.go.th",
    "URLJwks": "https://imauth.bora.dopa.go.th/jwks/",
    "Scope": "{Scope}",
    "ASEndPoint": "https://localhost:7228"
  }
}

```
---
## # Configuring Services for Making API Requests ##

location: `ThaIDAuthenExample/Program.cs`

Add Services for **Managing Sessions**.
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});
```
Add Services for **Connecting to Data on Another Server** via HTTP API.
```csharp
builder.Services.AddHttpClient("DOPA", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://imauthsbx.bora.dopa.go.th");
});
```
Add Services for **Integrating Data with ThaID**.
```csharp
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
```
---
## # Routing and Calling the Authentication Function Connected to ThaID ##
location: `ThaIDAuthenExample/Controllers/HomeController.cs`

Registering with DOPA requires the **Callback URL** to match the Route, ensuring that DOPA can send the **Authorization Code** correctly. You can update this setting in the **RP Admin website**.
```csharp
[Route("/authentication/login-callback")]
public async Task<IActionResult> Authentication(string code, string state)
{
    TokenResponse tokenResponse = await _authenticationService.RequestTokenAsync(code, state);
    CreateSessionToken(tokenResponse);
    return View("Authentication",tokenResponse);
}
```
**Function** for the **Home Page** of the web application.
```csharp
public IActionResult Index()
{
    return View();
}
```
**Function** for the **authentication process**, which redirects users to the **ThaID website** for verification.
```csharp
[Route("/authentication/login")]
public async Task<IActionResult> login()
{
    AuthorizeState provider = await _authenticationService.CreateProvider();
    return Redirect(provider.StartUrl);
}
```
**Function** for after the authentication process, which, upon receiving the **Authorization Code**, makes an **API Request** to obtain a **Token** from ThaID.
```csharp
[Route("/authentication/login-callback")]
public async Task<IActionResult> Authentication(string code, string state)
{
    TokenResponse tokenResponse = await _authenticationService.RequestTokenAsync(code, state);
    CreateSessionToken(tokenResponse);
    return View("Authentication",tokenResponse);
}
```
**Function** for requesting a **new Token** when the **Access Token** expires or becomes invalid, by calling the **API Refresh Token**.
```csharp
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
```
**Function** for verifying if the **ThaID ID** received from ThaID complies with **OpenID Connect** standards.
```csharp
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
```
**Function** for creating a **Session** to store the **Token** for the user.
```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

public void CreateSessionToken(TokenResponse tokenForSet)
{
    HttpContext.Session.SetString("token", JsonSerializer.Serialize(tokenForSet));
}
```
**Function** for sending a response in case of an **Error**.
```csharp
public IActionResult Error()
{
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
```
---
## # Token Model ##
location: `ThaIDAuthenExample/Models/TokenModel.cs`

**Variables** for storing **Token** values according to **OpenID Connect** standards.
```csharp
[JsonPropertyName("access_token")]
public required string AccessToken { get; set; }

[JsonPropertyName("refresh_token")]
public required string RefreshToken { get; set; }

[JsonPropertyName("token_type")]
public required string TokenType { get; set; }

[JsonPropertyName("expires_in")]
public required long ExpiresIn { get; set; }

[JsonPropertyName("scope")]
public required string Scope { get; set; }

[JsonPropertyName("id_token")]
public string? IDToken { get; set; }

public JwtSecurityToken IDTokenJWT
{
    get { return ConvertToJWT(IDToken); }
}
```
**Function** for converting a **JWT** string into a **JWT Data Object** for use or validation.
```csharp
private JwtSecurityToken ConvertToJWT(string token)
{
    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
    return handler.ReadJwtToken(token);
}
```
---
## # Error Management Model ##
location: `ThaIDAuthenExample/Models/ErrorViewModel.cs`

**Variables** for storing **Error** values.
```csharp
namespace ThaIDAuthenExample.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

```
---
## # Authentication Function connected to ThaID ##
location: `ThaIDAuthenExample/Services/AuthenticationService.cs`

**Construct Authentication Service** and configure the necessary settings for connecting data to **ThaID**.
```csharp
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
```
**Function** for starting the authentication process using the **IdentityModel.OidcClient** library.
```csharp
public async Task<AuthorizeState> CreateProvider()
{
    var configProvider = _provider.PrepareLoginAsync().Result;
    return configProvider;
}
```
**Function** for requesting a **Token** set after the **authentication process** with ThaID.
```csharp
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
```
**Function** for initiating the **authentication process**.
```csharp
public async Task<bool> ValidateIdToken(string keyForValidate, string idToken)
{
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
```
---
## 📁 ThaIDAuthenAPIExample ##
## # Settings for connecting to ThaID ##
location: `ThaIDAuthenAPIExample/appsettings.json`

Variables for **configuration** used with **ThaID** data integration, such as **client ID, client secret, scope**.
```json
{
  "ThaID": 
  {
    "ClientID": "{Client ID from DOPA}",
    "ClientSecret": "{Client Secret from DOPA}",
    "APIKey": "{API Key from DOPA}"
  }
}
```
---
## # Configuring Services for Making API Requests ##

location: `ThaIDAuthenAPIExample/Program.cs`

Add Services for **Connecting to Data on Another Server** via HTTP API.
```csharp
builder.Services.AddHttpClient("DOPA", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://imauth.bora.dopa.go.th");
});
```
---
## # Routing and invoking the authentication function connected to ThaID ##

location: `ThaIDAuthenAPIExample/Controllers/TokenInspectController.cs`


**Configure the Route** for the **TokenInspect** function to let **ThaIDAuthenExample** test its **availability** using the **API Introspect Token** for **Authorize Resources** from other systems using **ThaID**.

```csharp
[HttpGet(Name = "TokenInspect")]
public async Task<TokenInspect> Get()
{
    return await _authenticationService.TokenIntroSpectAsync(Request.Headers.Authorization);
}
```
---
## # Routing and calling the authentication function linked to ThaID ##
location: `ThaIDAuthenAPIExample/Controllers/TokenRevokeController.cs`

**Setting up the Route for the Function TokenRevoke** to allow **ThaIDAuthenExample** to test revoking the **Access Token**.
```csharp
[HttpGet(Name = "TokenRevoke")]
public async Task<TokenRevoke> Get()
{
    return await _authenticationService.TokenRevokeAsync(Request.Headers.Authorization);
}
```
---
## # Token Model ##

location: `ThaIDAuthenAPIExample/Models/TokenModel.cs`

**Variable** for **storing** values received from the **API Inspect Token**.
```csharp
public class TokenInspect
{
    [JsonPropertyName("active")]
    public required bool Active { get; set; }

    [JsonPropertyName("sub")]
    public string? SubjectIdentifier { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
```
**Variable** for **storing** values received from the **API Revoke Token**.
```csharp
public class TokenRevoke
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
```
---
## # Authentication Function connected to ThaID ##
location: `ThaIDAuthenAPIExample/Services/AuthenticationService.cs`

**Construct Authentication Service** and set up the necessary configurations for **connecting data to ThaID**.
```csharp
private readonly IHttpClientFactory _httpClientFactory;
private readonly IConfiguration _configuration;
public AuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    _httpClientFactory = httpClientFactory;
    _configuration = configuration;
}
```
**Function** to initiate the **authentication process** using the **IdentityModel.OidcClient** library.
```csharp
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
```
**Function** to request the **revocation** of an **Access Token** by calling the **API Revoke Token**.
```csharp
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
```
**Function** to **generat**e an **Authorization Key** for linking data with **ThaID**.
```csharp
private string ClientAuthen(string clientID, string clientSecret)
{
    byte[] clientAuthen = System.Text.Encoding.UTF8.GetBytes($"{clientID}:{clientSecret}");
    return Convert.ToBase64String(clientAuthen);
}
```