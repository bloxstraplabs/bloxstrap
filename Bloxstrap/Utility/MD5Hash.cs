using System.Security.Cryptography;

namespace Bloxstrap.Utility
{
    public static class MD5Hash
    {
        public static string FromBytes(byte[] data)
        {
            using MD5 md5 = MD5.Create();
            return Stringify(md5.ComputeHash(data));
        }

        public static string FromStream(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            return Stringify(md5.ComputeHash(stream));
        }

        public static string FromFile(string filename)
        {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(filename);
            return FromStream(stream);
        }

        private static string Stringify(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
