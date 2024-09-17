using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace ThaIDAuthenExample.Models
{

    public class TokenResponse
    {
        /// <summary>
        /// String มีอายุ 15 นาที
        /// </summary>
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        /// <summary>
        /// String มีอายุ 15 นาที
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public required string RefreshToken { get; set; }

        /// <summary>
        /// ใช้ค่า “Bearer”
        /// </summary>
        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        /// <summary>
        /// วัน เวลาที่ access token หมดอายุรูปแบบ Unix timestamp (second)
        /// </summary>
        [JsonPropertyName("expires_in")]
        public required long ExpiresIn { get; set; }

        /// <summary>
        /// Scope ของข้อมูลที่ได้รับ
        /// </summary>
        [JsonPropertyName("scope")]
        public required string Scope { get; set; }

        /// <summary>
        /// Option ถ้า scope=openid คือ id_token ที่มีข้อมูลผู้ใช้บริการที่ RP ร้องขอ
        /// </summary>
        [JsonPropertyName("id_token")]
        public string? IDToken { get; set; }

        public JwtSecurityToken IDTokenJWT
        {
            get { return ConvertToJWT(IDToken); }
        }

        

        private JwtSecurityToken ConvertToJWT(string token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
    }
}
