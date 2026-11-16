using System;
using System.Security.Cryptography;
namespace baitapBE_22DH111429.Models
{
    public static class PasswordUtility
    {
        public static string Hash(string password)
        {
            const int iterations = 100000; byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash; using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)) hash = pbkdf2.GetBytes(32);
            return "PBKDF2$" + iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }
    }
}
