using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace AliceScript.NameSpaces
{
    internal static class Alice_Security_Initer
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
                space.Add(new Password_VerifyData()) ;

                space.Add(new sha256_gethash());
                space.Add(new sha512_gethash());

                NameSpaceManerger.Add(space);
            }
            catch { }
        }
    }

    internal class PSS
    {
        public static int HASH_SIZE = 32;

        public static int STRETCH_COUNT = 1000;
    }
    internal class sha256_gethash : FunctionBase
    {
        public sha256_gethash()
        {
            this.Name = "sha256_gethash";
            this.MinimumArgCounts = 1;
            this.Run += Sha256_gethash_Run;
        }

        private void Sha256_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            var crypto = new SHA256CryptoServiceProvider();
            byte[] hashValue = crypto.ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal class sha512_gethash : FunctionBase
    {
        public sha512_gethash()
        {
            this.Name = "sha512_gethash";
            this.MinimumArgCounts = 1;
            this.Run += Sha256_gethash_Run;
        }

        private void Sha256_gethash_Run(object sender, FunctionBaseEventArgs e)
        {
            var crypto = new SHA512CryptoServiceProvider();
            byte[] hashValue = crypto.ComputeHash(e.Args[0].ByteArray);
            e.Return = new Variable(hashValue);
        }
    }
    internal class Password_Hash : FunctionBase
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
                ThrowErrorManerger.OnThrowError("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
                return;
            }
            string password = e.Args[0].ToString();

            // ソルトを取得
            byte[] salt = e.Args[0].AsByteArray();

            // ハッシュ値を取得
            byte[] hash =PasswordSaltHashManerger.GetHash(password, salt, Utils.GetSafeInt(e.Args,2,PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            e.Return = new Variable(hash);
        }

    }

    internal class Password_HashData : FunctionBase
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
                ThrowErrorManerger.OnThrowError("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
                return;
            }
           byte[] password = e.Args[0].AsByteArray();

            // ソルトを取得
            byte[] salt = e.Args[0].AsByteArray();

            // ハッシュ値を取得
            byte[] hash = PasswordSaltHashManerger.GetHashData(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            e.Return = new Variable(hash);
        }

    }

    internal class Password_Salt : FunctionBase
    {
        public Password_Salt()
        {
            Name = "password_salt";
            MinimumArgCounts = 0;
            Run += Class3_Run;
        }

        private void Class3_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(PasswordSaltHashManerger.GetSalt(Utils.GetSafeInt(e.Args,0,30)));
        }
        
    }

    internal class Password_Verify : FunctionBase
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
                ThrowErrorManerger.OnThrowError("引数が不正です",Exceptions.COULDNT_CONVERT_VARIABLE,e.Script);
                return;
            }
            string password = e.Args[0].AsString();

            // ソルトを取得
            byte[] salt = e.Args[2].AsByteArray();

            // ハッシュ値を取得
            byte[] hash =PasswordSaltHashManerger.GetHash(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            bool i = (e.Args[1].AsByteArray() == hash);
            e.Return = new Variable(i);
        }


    }

    internal class Password_VerifyData : FunctionBase
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
                ThrowErrorManerger.OnThrowError("引数が不正です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
                return;
            }
            byte[] password = e.Args[0].AsByteArray();

            // ソルトを取得
            byte[] salt = e.Args[2].AsByteArray();

            // ハッシュ値を取得
            byte[] hash = PasswordSaltHashManerger.GetHashData(password, salt, Utils.GetSafeInt(e.Args, 2, PSS.HASH_SIZE), Utils.GetSafeInt(e.Args, 3, PSS.STRETCH_COUNT));

            bool i = (e.Args[1].AsByteArray() == hash);
            e.Return = new Variable(i);
        }


    }
    internal static class PasswordSaltHashManerger
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
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(bytes);
            }
            return bytes;
        }
    }
}
