using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TodoAPI.Helpers
{
    public static class Hashing
    {
        public static string HashPassword(string Password)
        {
            var salt = "my-salt";
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: Password,
                    salt: Encoding.UTF8.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256/8
                )
            );
        }
    }
}