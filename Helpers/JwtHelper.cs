using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Config;

namespace TodoAPI.Helpers
{
    public static class JwtHelper
    {
        public static TokenValidationParameters GetJwtParamteres(JwtSettings settings)
        {
            return new TokenValidationParameters 
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key))
                };
        }

        public static bool IsValidToken(string token, JwtSettings settings)
        {
            try
            {
                SecurityToken tkn;
                var res = new JwtSecurityTokenHandler().ValidateToken(token, GetJwtParamteres(settings), out tkn);
                return true;
            }
            catch (System.Exception)
            {
                
                return false;
            }

        }
    }
}