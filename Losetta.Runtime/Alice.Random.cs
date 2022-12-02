using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript.NameSpaces
{
    static class Alice_Random_Initer
    {
        internal static Random random;
        public static void Init()
        {
            random = new Random();
            NameSpace space = new NameSpace("Alice.Random");

            space.Add(new randFunc());
            space.Add(new rand_bytesFunc());
            space.Add(new random_intFunc());
            space.Add(new rand_doubleFunc());
            space.Add(new random_bytesFunc());

            space.Add(new guid_new_textFunc());
            space.Add(new guid_new_bytesFunc());


            NameSpaceManerger.Add(space);
        }
    }
    class randFunc : FunctionBase
    {
        public randFunc()
        {
            this.Name = "rand";
            this.MinimumArgCounts = 0;
            this.Run += RandFunc_Run;
        }

        private void RandFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                e.Return = new Variable(Alice_Random_Initer.random.Next());
            }
            else if (e.Args.Count == 1)
            {
                e.Return = new Variable(Alice_Random_Initer.random.Next(e.Args[0].AsInt()));
            }
            else if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Random_Initer.random.Next(e.Args[0].AsInt(), e.Args[1].AsInt()));
            }
        }
    }
    class rand_bytesFunc : FunctionBase
    {
        public rand_bytesFunc()
        {
            this.Name = "rand_bytes";
            this.MinimumArgCounts = 1;
            this.Run += RandFunc_Run;
        }

        private void RandFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] bs = new byte[e.Args[0].AsInt()];
            Alice_Random_Initer.random.NextBytes(bs);
            e.Return = new Variable(bs);
        }
    }
    class rand_doubleFunc : FunctionBase
    {
        public rand_doubleFunc()
        {
            this.Name = "rand_double";
            this.MinimumArgCounts = 0;
            this.Run += RandFunc_Run;
        }

        private void RandFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice_Random_Initer.random.NextDouble());
        }
    }
    class random_intFunc : FunctionBase
    {
        public random_intFunc()
        {
            this.Name = "random_int";
            this.MinimumArgCounts = 0;
            this.Run += Random_intFunc_Run;
        }

        private void Random_intFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            if (e.Args.Count == 0)
            {
                var randomByte = new byte[4];
                using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                {
                    rng.GetBytes(randomByte);
                }
                e.Return = new Variable(BitConverter.ToInt32(randomByte, 0));
            }
            else if (e.Args.Count == 2)
            {
                //min、maxともにint型の範囲を逸脱していないことを確認
                int min = e.Args[0].AsInt();
                int max = e.Args[1].AsInt();

                //このとき選出されるべき数値の範囲は次のとおりである
                int com = min.CompareTo(max);
                byte[] randomByte;
                //選出されるべき範囲によって確保する領域を変更することによって高速化を図ります
                if (com <= 256)
                {
                    //256まで、すなわち1バイトで済む場合
                    randomByte = new byte[1];
                }
                else if (com <= 65536)
                {
                    //65536、2バイト
                    randomByte = new byte[2];
                }
                else if (com <= 16777216)
                {
                    //16777216、3バイト
                    randomByte = new byte[3];
                }
                else
                {
                    //それ以上、4バイト
                    randomByte = new byte[4];
                }
                int i = 0;
                do
                {
                    using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(randomByte);
                    }
                    Array.Resize(ref randomByte,4);
                    i = BitConverter.ToInt32(randomByte, 0);
                    i = i.CompareTo(min);
                }
                //範囲に収まるまで繰り返す
                while (!(i > min) || !(i < max));
                //答えを返します
                e.Return = new Variable(i);
            }
            else if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Random_Initer.random.Next(e.Args[0].AsInt(), e.Args[1].AsInt()));
            }
        }
    }
    class random_bytesFunc : FunctionBase
    {
        public random_bytesFunc()
        {
            this.Name = "random_bytes";
            this.MinimumArgCounts = 1;
            this.Run += Random_bytesFunc_Run;
        }

        private void Random_bytesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if ((Utils.GetSafeInt(e.Args, 1) == 0))
            {
                var randomByte = new byte[e.Args[0].AsInt()];
                using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                {
                    rng.GetBytes(randomByte);
                }
                e.Return = new Variable(randomByte);
            }
            else
            {
                var randomByte = new byte[e.Args[0].AsInt()];
                using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
                {
                    rng.GetNonZeroBytes(randomByte);
                }
                e.Return = new Variable(randomByte);
            }
        }
    }
    class guid_new_textFunc : FunctionBase
    {
        public guid_new_textFunc()
        {
            this.Name = "guid_new_text";
            this.Run += GuidFunc_Run;
        }

        private void GuidFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Guid.NewGuid().ToString());
        }
    }
    class guid_new_bytesFunc : FunctionBase
    {
        public guid_new_bytesFunc()
        {
            this.Name = "guid_new_bytes";
            this.Run += GuidFunc_Run;
        }

        private void GuidFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Guid.NewGuid().ToByteArray());
        }
    }
}