using AliceScript.Binding;
using System.Security.Cryptography;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Security
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(SecurityFunctions));
        }
    }
    [AliceNameSpace(Name = "Alice.Security")]
    internal static class SecurityFunctions
    {
        #region バイト配列暗号化
        public static byte[] Data_Encrypt(byte[] data, string password)
        {
            return FileEncrypter.Encrypt(data, password);
        }
        public static byte[] Data_Decrypt(byte[] data, string password)
        {
            return FileEncrypter.Decrypt(data, password);
        }
        #endregion

        #region ハッシュ関数
        public static byte[] MD5_GetHash(byte[] data)
        {
            return MD5.HashData(data);
        }
        public static byte[] SHA1_GetHash(byte[] data)
        {
            return SHA1.HashData(data);
        }
        public static byte[] SHA256_GetHash(byte[] data)
        {
            return SHA256.HashData(data);
        }
        public static byte[] SHA384_GetHash(byte[] data)
        {
            return SHA384.HashData(data);
        }
        public static byte[] SHA512_GetHash(byte[] data)
        {
            return SHA512.HashData(data);
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
        internal static byte[] GetSalt(int size)
        {
            var bytes = new byte[size];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }
    }
}