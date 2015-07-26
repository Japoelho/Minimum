using System;
using System.Security.Cryptography;

namespace Minimum.Cryptography
{
    public class AES
    {
        public static string Encrypt(string message, string password)
        {
            string result = null;

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            AesManaged aes = new AesManaged();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform transform = aes.CreateEncryptor())
            {
                byte[] encryptedBytes = transform.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                result = Convert.ToBase64String(encryptedBytes);
            }

            return result;
        }

        public static string Decrypt(string message, string password)
        {
            string result = null;

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] messageBytes = Convert.FromBase64String(message);

            AesManaged aes = new AesManaged();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform transform = aes.CreateDecryptor())
            {
                byte[] decryptedBytes = transform.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                result = System.Text.Encoding.UTF8.GetString(decryptedBytes);
            }

            return result;
        }
    }
}
