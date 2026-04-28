using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;  
using System.Text;

namespace PAN.API.Utilities
{
    public class EncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration config)
        {
            _key = Encoding.UTF8.GetBytes(config["Encryption:Key"]);
            _iv = Encoding.UTF8.GetBytes(config["Encryption:IV"]);
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();   
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor();

            var bytes = Encoding.UTF8.GetBytes(plainText ?? "");
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";

            var bytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();   
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor();

            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}