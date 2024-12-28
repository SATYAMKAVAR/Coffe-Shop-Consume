using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Coffee_Shop_Management_System.Helper
{
    public static class UrlEncryptor
    {
        private static readonly string EncryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "pjsGLNYrMqU6wny4"; // Use secure storage in production.

        static UrlEncryptor()
        {
            if (EncryptionKey.Length != 16 && EncryptionKey.Length != 24 && EncryptionKey.Length != 32)
                throw new ArgumentException("Encryption key must be 16, 24, or 32 bytes long.");
        }

        public static string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text to encrypt cannot be null or empty.");

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aesAlg.GenerateIV(); // Generate a random IV

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // Prepend the IV to the ciphertext

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty.");

            if (!IsBase64String(encryptedText))
                throw new FormatException("The input is not a valid Base64 string.");

            var fullCipher = Convert.FromBase64String(encryptedText);

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);

                // Extract the IV from the first 16 bytes of the ciphertext
                byte[] iv = new byte[16];
                Array.Copy(fullCipher, iv, iv.Length);
                aesAlg.IV = iv;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(fullCipher, 16, fullCipher.Length - 16)) // Skip the IV
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        private static bool IsBase64String(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }
}
