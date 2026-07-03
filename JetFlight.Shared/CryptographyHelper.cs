using System.Security.Cryptography;
using System.Text;

namespace JetFlight.Shared
{
    public static class CryptographyHelper
    {
        private const int KeySize = 64;
        private const int Iterations = 350000;
        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA512;

        public static string HashPassword(string password, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(KeySize);
            return HashPassword(password, salt);
        }

        public static string HashPassword(string password, byte[] salt)
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                _hashAlgorithm,
                KeySize);
            return Convert.ToHexString(hash);
        }
    }
}
