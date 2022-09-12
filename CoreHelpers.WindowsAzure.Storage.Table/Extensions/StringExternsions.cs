using System;
using System.Text;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public static class StringExternsions
    {
        public static string ToBase64(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string ToSha256(this string value)
        {
            // convert the string 
            byte[] toBytes = Encoding.ASCII.GetBytes(value);

            // generate the SHA1 key
            var sha256 = global::System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(toBytes);

            // done
            return GenerateHexStringFromBytes(hash);
        }

        private static string GenerateHexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}

