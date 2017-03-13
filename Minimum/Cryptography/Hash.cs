using System.Security.Cryptography;
using System.Text;

namespace Minimum.Cryptography
{
    public class Hash
    {
        public static string GenerateHash(string original)
        {
            HashAlgorithm algorithm = MD5.Create();
            byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(original));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hash)
            {
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString();
        }
    }
}