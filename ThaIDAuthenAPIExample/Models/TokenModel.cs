using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace ThaIDAuthenAPIExample.Models
{

    public class TokenInspect
    {
        /// <summary>
        /// สถานะของ Token
        /// </summary>
        [JsonPropertyName("active")]
        public required bool Active { get; set; }

        /// <summary>
        /// Subject Identifier
        /// </summary>
        [JsonPropertyName("sub")]
        public string? SubjectIdentifier { get; set; }

        /// <summary>
        /// Scope ของข้อมูลที่ได้รับ
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    public class TokenRevoke
    {
        ///// <summary>
        ///// สถานะของ Token
        ///// </summary>
        //[JsonPropertyName("status")]
        //public required string Status { get; set; }

        /// <summary>
        /// ข้อความจาก Identity Provider
        /// </summary>
        [JsonPropertyName("message")]
        public required string Message { get; set; }

    }

}
