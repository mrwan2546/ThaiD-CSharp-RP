# 1. คำแนะนำเพื่อใช้งานแอปพลิเคชัน ThaIDAuthenExample เพื่อทดสอบการเชื่อมต่อระบบ ThaID ด้วยภาษา C# ASP.NET 8

แอปพลิเคชันเป็นตัวอย่างเพื่อแสดงวิธีการเชื่อมต่อ **ThaID** โดยใช้ภาษา **C#** ร่วมกับเฟรมเวิร์ก **ASP.NET 8** โดยใช้การยืนยันตัวตนด้วยมาตรฐาน **OpenID Connect & OAuth2**

โซลูชันนี้มีสองโปรเจกต์

- ThaIDAuthenExample: ใช้ทดสอบการเชื่อมต่อกับ ThaID
- ThaIDAuthenAPIExample: สำหรับจำลองเป็น Authorize Resource เพื่อขอยิง API ข้ามระบบเพื่อทดสอบ

## # 📁 library ในโปรเจกต์

1. **IdentityModel.OidcClient** สำหรับต่อ OAuth2 ของ ThaID
2. **System.IdentityModel.Tokens.Jwt** สำหรับต่อ Validate id token

## # 📁 Runtime

1. **.NET 8.0 Runtime** (รวมอยู่ในชุดพัฒนาโปรแกรม **Visual Studio Community 2022**)

## # 📁🎛️ การติดตั้ง และตั้งค่าโปรแกรม Visual Studio Community 2022 สำหรับใช้งานแอปพลิเคชัน

1. ดาวน์โหลดโปรแกรม **Visual Studio Installer** สำหรับติดตั้งโปรแกรม **Visual Studio Community 2022** โดยกดที่ [link download](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false)
2. เปิดโปรแกรม **Visual Studio Installer** ให้เลือกโหลด **Visual Studio Community 2022** และเลือก **ASP.NET and web development** กด **download** และรอจนกว่าการติดตั้งจะเสร็จสมบูรณ์
3. เปิดโปรแกรม **Visual Studio Community 2022** และเลือก **Open a project or solution**
4. ค้นหาไฟล์โปรเจกต์ชื่อ `ThaIDAuthenExample.sln` ในโฟลเดอร์ `ThaID\CSharp\ThaIDAuthenExample`
5. ตั้งค่าสำหรับการเชื่อมต่อ **ThaID** ตามรายละเอียดดังนี้

---

location: `ThaIDAuthenExample/appsettings.json`
แก่ไขค่าในตัวแปรสำหรับการเชื่อมโยงข้อมูล **ThaID** ได้แก่ **client id, client secret, API Key, Callback URL, Scope** ตามตัวอย่างในไฟล์ JSON

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

6. เลือกเมนู **Startup Item** (สัญลักษณ์ **ฟันเฟือง**⚙️) และเลือก **ThaIDAuthenExample** สำหรับเปิด Web หรือ **ThaIDAuthenAPIExample** สำหรับเปิด API
7. กดปุ่ม **https** (สัญลักษณ์ **RUN**▶️ สีเขียว)

<br><br><br>

# 2. องค์ประกอบของแอปพลิเคชันภายในโซลูชัน

## 📁 ThaIDAuthenExample

## # การตั้งค่า **Service** เพื่อการเรียกใช้งาน API

location: `ThaIDAuthenExample/Program.cs`

**Services** สำหรับการจัดการ **Session**

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});
```

**Services** สำหรับ **เชื่อมต่อ** ข้อมูลเครื่อง **Server อื่น** ผ่าน HTTP Rest API ซึ่งในตัวอย่างเป็น Server ThaID

```csharp
builder.Services.AddHttpClient("DOPA", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://imauthsbx.bora.dopa.go.th");
});
```

**Services** สำหรับ **เชื่อมโยง** ข้อมูลกับ **ThaID**

```csharp
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
```

---

## # การตั้งค่า Route และการเรียกใช้ฟังก์ชันการยืนยันตัวตนที่เชื่อมโยงกับ ThaID

location: `ThaIDAuthenExample/Controllers/HomeController.cs`

การลงทะเบียน กับ DOPA ค่า **Callback URL** จะต้องตรงกับค่า Route เพื่อให้ DOPA ส่ง **Authorize Code** ได้ถูกต้อง ซึ่งสามารถแก้การตั้งค่าได้ที่เว็บ **RP Admin**

```csharp
[Route("/authentication/login-callback")]
public async Task<IActionResult> Authentication(string code, string state)
{
    TokenResponse tokenResponse = await _authenticationService.RequestTokenAsync(code, state);
    CreateSessionToken(tokenResponse);
    return View("Authentication",tokenResponse);
}
```

**Function** สำหรับ **หน้าแรก** ของเว็บแอปพลิเคชัน

```csharp
public IActionResult Index()
{
    return View();
}
```

**Function** สำหรับเข้ากระบวนการ **ยืนยันตัวตน** โดยระบบจะพาผู้ใช้ไป **เว็บไซต์ ThaID** เพื่อยืนยันตัวตน

```csharp
[Route("/authentication/login")]
public async Task<IActionResult> login()
{
    AuthorizeState provider = await _authenticationService.CreateProvider();
    return Redirect(provider.StartUrl);
}
```

**Function** สำหรับหลังจากเข้ากระบวนการยืนยันตัวตนแล้ว และได้รับ **Authorization Code** จะเรียก **API Request Token** เพื่อร้องขอชุด **Token** มาจาก ThaID

```csharp
[Route("/authentication/login-callback")]
public async Task<IActionResult> Authentication(string code, string state)
{
    TokenResponse tokenResponse = await _authenticationService.RequestTokenAsync(code, state);
    CreateSessionToken(tokenResponse);
    return View("Authentication",tokenResponse);
}
```

**Function** สำหรับร้องขอ **Token ชุดใหม่** ในกรณี **Access Token** หมดอายุ หรือใช้งานไม่ได้แล้วโดยเรียกจาก **API Refresh Token**

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

**Function** สำหรับตรวจสอบ **ID Token** ที่ได้รับจาก ThaID ว่าถูกต้องตามมาตรฐาน **OpenID Connect**

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

**Function** สำหรับการสร้าง **Session** เพื่อจัดเก็บ **Token** ให้กับผู้ใช้

```csharp
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

public void CreateSessionToken(TokenResponse tokenForSet)
{
    HttpContext.Session.SetString("token", JsonSerializer.Serialize(tokenForSet));
}
```

**Function** สำหรับการส่งข้อมูลกลับในกรณีมี **Error** เกิดขึ้น

```csharp
public IActionResult Error()
{
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
```

---

## # Model ของ Token

location: `ThaIDAuthenExample/Models/TokenModel.cs`

**ตัวแปร** สำหรับการเก็บค่า **Token** ตามมาตรฐาน **OpenID Connect**

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

**Function** สำหรับการแปลงข้อมูล **JWT** ที่เป็น String เป็น **JWT Data Object** เพื่อนำไปใช้งานหรือตรวจสอบความถูกต้อง

```csharp
private JwtSecurityToken ConvertToJWT(string token)
{
    JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
    return handler.ReadJwtToken(token);
}
```

---

## # Model ของการจัดการ Error

location: `ThaIDAuthenExample/Models/ErrorViewModel.cs`

**ตัวแปร** สำหรับการเก็บค่า **Error**

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

## # ฟังก์ชันการยืนยันตัวตนที่เชื่อมโยงกับ ThaID

location: `ThaIDAuthenExample/Services/AuthenticationService.cs`

**ตั้งค่าเริ่มต้นของ Authentication Service** และค่าต่าง ๆ ที่จำเป็นต่อการเชื่อมโยงข้อมูลไปยัง **ThaID**

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

**Function** สำหรับเริ่มกระบวนการยินยันตัวตนด้วย Library **IdentityModel.OidcClient**

```csharp
public async Task<AuthorizeState> CreateProvider()
{
    var configProvider = _provider.PrepareLoginAsync().Result;
    return configProvider;
}
```

**Function** สำหรับการร้องขอชุด **Token** หลังจากกระบวนการ **ยืนยันตัวตน** ของ ThaID

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

**Function** สำหรับเริ่มกระบวนการ **ยืนยันตัวตน**

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

## 📁 ThaIDAuthenAPIExample

## # การตั้งค่าเพื่อเชื่อมโยง ThaID

location: `ThaIDAuthenAPIExample/appsettings.json`

**ตัวแปร** สำหรับการ **ตั้งค่า** ที่ใช้กับการเชื่อมโยงข้อมูล **ThaID** เช่น **client id, client secret, API Key** เป็นต้น

```json
{
  "ThaID": {
    "ClientID": "{Client ID from DOPA}",
    "ClientSecret": "{Client Secret from DOPA}",
    "APIKey": "{API Key from DOPA}"
  }
}
```

---

## # การตั้งค่า Service ที่ใช้ในการเรียกใช้งาน API

location: `ThaIDAuthenAPIExample/Program.cs`

เพิ่ม Services ในการ **เชื่อมต่อข้อมูลเครื่อง Server** อื่นผ่าน HTTP API

```csharp
builder.Services.AddHttpClient("DOPA", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://imauth.bora.dopa.go.th");
});
```

---

## # การ Route และการเรียกใช้ฟังก์ชันการยืนยันตัวตนที่เชื่อมโยงกับ ThaID

location: `ThaIDAuthenAPIExample/Controllers/TokenInspectController.cs`

**การตั้งค่า Route** สำหรับ Function **TokenInspect** เพื่อให้ **ThaIDAuthenExample** ทดสอบว่ายัง **ใช้งานได้หรือไม่** ซึ่งใช้ **API Introspect Token** ในกรณีที่เป็น **Authorize Resource** และได้รับคำร้องขอข้อมูลมาจากระบบอื่นที่ใช้งาน **ThaID**

```csharp
[HttpGet(Name = "TokenInspect")]
public async Task<TokenInspect> Get()
{
    return await _authenticationService.TokenIntroSpectAsync(Request.Headers.Authorization);
}
```

---

## # การ Route และการเรียกใช้ฟังก์ชันการยกเลิกการใช้งาน Access Token

location: `ThaIDAuthenAPIExample/Controllers/TokenRevokeController.cs`

**การตั้งค่า Route สำหรับ Function TokeRevoke** เพื่อให้ **ThaIDAuthenExample** ทดสอบยกเลิกการใช้งาน **Access Token**

```csharp
[HttpGet(Name = "TokenRevoke")]
public async Task<TokenRevoke> Get()
{
    return await _authenticationService.TokenRevokeAsync(Request.Headers.Authorization);
}
```

---

## # Model ของ Token

location: `ThaIDAuthenAPIExample/Models/TokenModel.cs`

**ตัวแปร** สำหรับการ **เก็บค่า** ที่ส่งมาจาก **API Inspect Token**

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

**ตัวแปร** สำหรับการ **เก็บค่า** ที่ส่งมาจาก **API Revoke Token**

```csharp
public class TokenRevoke
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
```

---

## # ฟังก์ชันการยืนยันตัวตนที่เชื่อมโยงกับ ThaID

location: `ThaIDAuthenAPIExample/Services/AuthenticationService.cs`

**การตั้งค่าเริ่มต้น Authentication Service** และค่าต่าง ๆ ที่จำเป็นต่อ **การเชื่อมโยงข้อมูลไปยัง ThaID**

```csharp
private readonly IHttpClientFactory _httpClientFactory;
private readonly IConfiguration _configuration;
public AuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    _httpClientFactory = httpClientFactory;
    _configuration = configuration;
}
```

**Function** สำหรับเริ่มกระบวนการ **ยืนยันตัวตน** ด้วย Library **IdentityModel.OidcClient**

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

**Function** สำหรับร้องขอ **เพิกถอน Access Token** โดยเรียกจาก **API Revoke Token**

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

**Function** สำหรับการ **สร้าง Authorization Key** เพื่อใช้ในการเชื่อมโยงข้อมูลกับ **ThaID**

```csharp
private string ClientAuthen(string clientID, string clientSecret)
{
    byte[] clientAuthen = System.Text.Encoding.UTF8.GetBytes($"{clientID}:{clientSecret}");
    return Convert.ToBase64String(clientAuthen);
}
```
