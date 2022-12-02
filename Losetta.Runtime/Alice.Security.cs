using System.Security.Cryptography;
using System.Text;

namespace AliceScript.NameSpaces
{
    internal static class Alice_Security_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Security");

            space.Add(new Password_Hash());
            space.Add(new Password_Salt());
            space.Add(new Password_Verify());

            NameSpaceManerger.Add(space);
        }
    }

    internal class PSS
    {
        public static int SALT_SIZE = 32;

        public static int HASH_SIZE = 32;

        public static int STRETCH_COUNT = 1000;
    }

    internal class Password_Hash : FunctionBase
    {

        public Password_Hash()
        {
            FunctionName = "password_hash";
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
            byte[] hash =PasswordSaltHashManerger.GetHash(password, salt, PSS.HASH_SIZE, PSS.STRETCH_COUNT);

            e.Return = new Variable(hash);
        }

    }

    internal class Password_Salt : FunctionBase
    {
        public Password_Salt()
        {
            FunctionName = "password_salt";
            MinimumArgCounts = 0;
            Run += Class3_Run;
        }

        private void Class3_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(PasswordSaltHashManerger.GetSalt(PSS.SALT_SIZE));
        }
        
    }

    internal class Password_Verify : FunctionBase
    {

        public Password_Verify()
        {
            FunctionName = "password_verify";
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
            byte[] hash =PasswordSaltHashManerger.GetHash(password, salt, PSS.HASH_SIZE, PSS.STRETCH_COUNT);

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
