using AliceScript.Binding;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Security
    {
        public static void Init()
        {
            Alice.RegisterFunctions<SecurityFunctions>();
        }
    }
    [AliceNameSpace(Name = "Alice.Security")]
    internal sealed class SecurityFunctions
    {
        #region バイト配列暗号化
        public static byte[] Data_Encrypt(byte[] data, string password, int keySize = 128, int iterations = 1024, bool useSHA512 = false)
        {
            return FileEncrypter.Encrypt(data, password, keySize, iterations, useSHA512);
        }
        public static byte[] Data_Decrypt(byte[] data, string password, int keySize = 128, int iterations = 1024, bool useSHA512 = false)
        {
            return FileEncrypter.Decrypt(data, password, keySize, iterations, useSHA512);
        }
        #endregion

        #region ハッシュ関数
#if !NET5_0_OR_GREATER
        // もし必要な場合はハッシュアルゴリズムをキャッシュしておく
        private static HashAlgorithm MD5Algorithm { get; set; }
        private static HashAlgorithm SHA1Algorithm { get; set; }
        private static HashAlgorithm SHA256Algorithm { get; set; }
        private static HashAlgorithm SHA384Algorithm { get; set; }
        private static HashAlgorithm SHA512Algorithm { get; set; }
#endif
        public static byte[] MD5_GetHash(byte[] data)
        {
#if NET5_0_OR_GREATER
            return MD5.HashData(data);
#else 
            MD5Algorithm = MD5Algorithm ?? MD5.Create();
            return MD5Algorithm.ComputeHash(data);
#endif
        }
        public static byte[] SHA1_GetHash(byte[] data)
        {
#if NET5_0_OR_GREATER
            return SHA1.HashData(data);
#else 
            SHA1Algorithm = SHA1Algorithm ?? SHA1.Create();
            return SHA1Algorithm.ComputeHash(data);
#endif
        }
        public static byte[] SHA256_GetHash(byte[] data)
        {
#if NET5_0_OR_GREATER
            return SHA256.HashData(data);
#else 
            SHA256Algorithm = SHA256Algorithm ?? SHA256.Create();
            return SHA256Algorithm.ComputeHash(data);
#endif
        }
        public static byte[] SHA384_GetHash(byte[] data)
        {
#if NET5_0_OR_GREATER
            return SHA384.HashData(data);
#else 
            SHA384Algorithm = SHA384Algorithm ?? SHA384.Create();
            return SHA384Algorithm.ComputeHash(data);
#endif
        }
        public static byte[] SHA512_GetHash(byte[] data)
        {
#if NET5_0_OR_GREATER
            return SHA512.HashData(data);
#else 
            SHA512Algorithm = SHA512Algorithm ?? SHA512.Create();
            return SHA512Algorithm.ComputeHash(data);
#endif
        }
        #endregion

        #region パスワード認証
        private const int HASH_SIZE = 32;

        private const int STRETCH_COUNT = 1000;

        public static byte[] Password_Hash(string password, byte[] salt)
        {
            return PasswordSaltHashManager.GetHash(password, salt, HASH_SIZE, STRETCH_COUNT);
        }
        public static byte[] Password_Hash(string password, byte[] salt, int size)
        {
            return PasswordSaltHashManager.GetHash(password, salt, size, STRETCH_COUNT);
        }
        public static byte[] Password_Hash(string password, byte[] salt, int size, int cnt)
        {
            return PasswordSaltHashManager.GetHash(password, salt, size, cnt);
        }
        public static byte[] Password_HashData(byte[] password, byte[] salt)
        {
            return PasswordSaltHashManager.GetHashData(password, salt, HASH_SIZE, STRETCH_COUNT);
        }
        public static byte[] Password_HashData(byte[] password, byte[] salt, int size)
        {
            return PasswordSaltHashManager.GetHashData(password, salt, size, STRETCH_COUNT);
        }
        public static byte[] Password_HashData(byte[] password, byte[] salt, int size, int cnt)
        {
            return PasswordSaltHashManager.GetHashData(password, salt, size, cnt);
        }
        public static byte[] Password_GetSalt()
        {
            return PasswordSaltHashManager.GetSalt(30);
        }
        public static byte[] Password_GetSalt(int length)
        {
            return PasswordSaltHashManager.GetSalt(length);
        }
        public static bool Password_Verify(string password, byte[] hash, byte[] salt)
        {
            return PasswordSaltHashManager.GetHash(password, salt, HASH_SIZE, STRETCH_COUNT) == hash;
        }
        public static bool Password_Verify(string password, byte[] hash, byte[] salt, int size)
        {
            return PasswordSaltHashManager.GetHash(password, salt, size, STRETCH_COUNT) == hash;
        }
        public static bool Password_Verify(string password, byte[] hash, byte[] salt, int size, int cnt)
        {
            return PasswordSaltHashManager.GetHash(password, salt, size, cnt) == hash;
        }
        public static bool Password_VerifyData(byte[] password, byte[] hash, byte[] salt)
        {
            return PasswordSaltHashManager.GetHashData(password, salt, HASH_SIZE, STRETCH_COUNT) == hash;
        }
        public static bool Password_VerifyData(byte[] password, byte[] hash, byte[] salt, int size)
        {
            return PasswordSaltHashManager.GetHashData(password, salt, size, STRETCH_COUNT) == hash;
        }
        public static bool Password_VerifyData(byte[] password, byte[] hash, byte[] salt, int size, int cnt)
        {
            return PasswordSaltHashManager.GetHashData(password, salt, size, cnt) == hash;
        }
        #endregion
    }
    internal sealed class PasswordSaltHashManager
    {
        internal static byte[] GetHash(string password, byte[] salt, int size, int cnt)
        {
            byte[] bytesSalt;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, cnt))
            {
                bytesSalt = rfc2898DeriveBytes.GetBytes(size);
            }
            return bytesSalt;
        }
        internal static byte[] GetHashData(byte[] password, byte[] salt, int size, int cnt)
        {
            byte[] bytesSalt;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, cnt))
            {
                bytesSalt = rfc2898DeriveBytes.GetBytes(size);
            }
            return bytesSalt;
        }
#if !NETCOREAPP2_1_OR_GREATER
        private static RandomNumberGenerator RandomNumberGenerator { get; set; }
#endif
        internal static byte[] GetSalt(int size)
        {
#if NETCOREAPP2_1_OR_GREATER
            var bytes = new byte[size];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
#else
            RandomNumberGenerator = RandomNumberGenerator ?? RandomNumberGenerator.Create();
            var bytes = new byte[size];
            RandomNumberGenerator.GetBytes(bytes);
            return bytes;
#endif
        }
    }
}