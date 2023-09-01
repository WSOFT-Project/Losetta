using System.Security.Cryptography;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Security
    {
        public static void Init()
        {
            try
            {
                NameSpace space = new NameSpace("Alice.Security");

                space.Add(new Password_Hash());
                space.Add(new Password_Salt());
                space.Add(new Password_Verify());
                space.Add(new Password_HashData());
                space.Add(new Password_VerifyData());

                space.Add(new md5_gethash());
                space.Add(new sha1_gethash());
                space.Add(new sha256_gethash());
                space.Add(new sha384_gethash());
                space.Add(new sha512_gethash());

                space.Add(new file_encrypt_dataFunc());
                space.Add(new file_decrypt_dataFunc());

                NameSpaceManager.Add(space);
            }
            catch { }
        }
    }
    internal sealed class file_encrypt_dataFunc : FunctionBase
    {
        public file_encrypt_dataFunc()
        {
            Name = "encrypt_data";
            MinimumArgCounts = 2;
            Run += File_encrypt_dataFunc_Run;
        }

        private void File_encrypt_dataFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(FileEncrypter.Encrypt(e.Args[0].AsByteArray(), e.Args[1].AsString()));
        }
    }

    internal sealed class file_decrypt_dataFunc : FunctionBase
    {
        public file_decrypt_dataFunc()
        {
            Name = "decrypt_data";
            MinimumArgCounts = 2;
            Run += File_encrypt_dataFunc_Run;
        }

        private void File_encrypt_dataFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(FileEncrypter.Decrypt(e.Args[0].AsByteArray(), e.Args[1].AsString()));
        }
    }
    internal sealed class PSS
    {
        public static int HASH_SIZE = 32;

        public static int STRETCH_COUNT = 1000;
    }
    internal sealed class sha256_gethash : FunctionBase
    {
        public sha256_gethash()
        {
            Name = "sha256_gethash";
            MinimumArgCounts = 1;
            Run += Sha256_gethash_Run;
        }

        private void Sha256_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] hashValue = SHA256.Create().ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal sealed class sha384_gethash : FunctionBase
    {
        public sha384_gethash()
        {
            Name = "sha384_gethash";
            MinimumArgCounts = 1;
            Run += Sha256_gethash_Run;
        }

        private void Sha256_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] hashValue = SHA384.Create().ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal sealed class sha512_gethash : FunctionBase
    {
        public sha512_gethash()
        {
            Name = "sha512_gethash";
            MinimumArgCounts = 1;
            Run += Sha256_gethash_Run;
        }

        private void Sha256_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] hashValue = SHA512.Create().ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal sealed class sha1_gethash : FunctionBase
    {
        public sha1_gethash()
        {
            Name = "sha1_gethash";
            MinimumArgCounts = 1;
            Run += Sha256_gethash_Run;
        }

        private void Sha256_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] hashValue = SHA1.Create().ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal sealed class md5_gethash : FunctionBase
    {
        public md5_gethash()
        {
            Name = "md5_gethash";
            MinimumArgCounts = 1;
            Run += Md5_gethash_Run;
        }

        private void Md5_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] hashValue = MD5.Create().ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal sealed class Password_Hash : FunctionBase
    {

        public Password_Hash()
        {
            Name = "password_hash";
            MinimumArgCounts = 2;
            Run += Class1_Run;
        }

        private void Class1_Run(object sender, FunctionBaseEventArgs e)
        {
            //引数チェック
            if (e.Args[0].Type != Variable.VarType.STRING || e.Args[1].Type != Variable.VarType.BYTES || e.Args[1].ByteArray == null)
            {
                throw new ScriptException("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
            }
            string password = e.Args[0].ToString();

            // ソルトを取得
            byte[] salt = e.Args[0].AsByteArray();

            // ハッシュ値を取得
            byte[] hash = PasswordSaltHashManager.GetHash(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            e.Return = new Variable(hash);
        }

    }

    internal sealed class Password_HashData : FunctionBase
    {

        public Password_HashData()
        {
            Name = "password_hash_data";
            MinimumArgCounts = 2;
            Run += Class1_Run;
        }

        private void Class1_Run(object sender, FunctionBaseEventArgs e)
        {
            //引数チェック
            if (e.Args[0].Type != Variable.VarType.BYTES || e.Args[1].Type != Variable.VarType.BYTES || e.Args[1].ByteArray == null)
            {
                throw new ScriptException("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
            }
            byte[] password = e.Args[0].AsByteArray();

            // ソルトを取得
            byte[] salt = e.Args[0].AsByteArray();

            // ハッシュ値を取得
            byte[] hash = PasswordSaltHashManager.GetHashData(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            e.Return = new Variable(hash);
        }

    }

    internal sealed class Password_Salt : FunctionBase
    {
        public Password_Salt()
        {
            Name = "password_salt";
            MinimumArgCounts = 0;
            Run += Class3_Run;
        }

        private void Class3_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(PasswordSaltHashManager.GetSalt(Utils.GetSafeInt(e.Args, 0, 30)));
        }

    }

    internal sealed class Password_Verify : FunctionBase
    {

        public Password_Verify()
        {
            Name = "password_verify";
            MinimumArgCounts = 3;
            Run += Class1_Run;
        }

        private void Class1_Run(object sender, FunctionBaseEventArgs e)
        {
            //引数チェック
            if (e.Args[0].Type != Variable.VarType.STRING || e.Args[1].Type != Variable.VarType.BYTES || e.Args[1].ByteArray == null || e.Args[2].Type != Variable.VarType.BYTES || e.Args[2].ByteArray == null)
            {
                throw new ScriptException("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
            }
            string password = e.Args[0].AsString();

            // ソルトを取得
            byte[] salt = e.Args[2].AsByteArray();

            // ハッシュ値を取得
            byte[] hash = PasswordSaltHashManager.GetHash(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            bool i = e.Args[1].AsByteArray() == hash;
            e.Return = new Variable(i);
        }


    }

    internal sealed class Password_VerifyData : FunctionBase
    {

        public Password_VerifyData()
        {
            Name = "password_verify_data";
            MinimumArgCounts = 3;
            Run += Class1_Run;
        }

        private void Class1_Run(object sender, FunctionBaseEventArgs e)
        {
            //引数チェック
            if (e.Args[0].Type != Variable.VarType.BYTES || e.Args[1].Type != Variable.VarType.BYTES || e.Args[1].ByteArray == null || e.Args[2].Type != Variable.VarType.BYTES || e.Args[2].ByteArray == null)
            {
                throw new ScriptException("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
            }
            byte[] password = e.Args[0].AsByteArray();

            // ソルトを取得
            byte[] salt = e.Args[2].AsByteArray();

            // ハッシュ値を取得
            byte[] hash = PasswordSaltHashManager.GetHashData(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            bool i = e.Args[1].AsByteArray() == hash;
            e.Return = new Variable(i);
        }


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