using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace FinanceApp2.Web.Helpers
{
    public static class JwtTokenHelpers
    {
        public static bool IsTokenExpiredOrExpiring(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return true;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

                return jwtSecurityToken.ValidTo <= DateTime.UtcNow.AddMinutes(2);
            }
            catch
            {
                return true;
            }
        }

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs.Select(kvp =>
            {
                var type = kvp.Key switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "emailaddress" => ClaimTypes.Email,
                    _ => kvp.Key
                };

                return new Claim(type, kvp.Value.ToString() ?? "");
            });
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}