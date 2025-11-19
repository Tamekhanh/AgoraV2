using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Agora.Application.Utils
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

}
