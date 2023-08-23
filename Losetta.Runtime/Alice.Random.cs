using System.Security.Cryptography;

namespace AliceScript.NameSpaces
{
    internal sealed class  Alice_Random_Initer
    {
        internal static Random random;
        public static void Init()
        {
            try
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
            catch { }
        }
    }

    internal sealed class randFunc : FunctionBase
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

    internal sealed class rand_bytesFunc : FunctionBase
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

    internal sealed class rand_doubleFunc : FunctionBase
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

    internal sealed class random_intFunc : FunctionBase
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
                e.Return = new Variable(RandomNumberGenerator.GetInt32(int.MaxValue));
            }
            else if (e.Args.Count == 1)
            {
                e.Return = new Variable(RandomNumberGenerator.GetInt32(e.Args[0].AsInt()));
            }
            else if (e.Args.Count == 2)
            {
                e.Return = new Variable(RandomNumberGenerator.GetInt32(e.Args[0].AsInt(), e.Args[1].AsInt()));
            }
        }
    }

    internal sealed class random_bytesFunc : FunctionBase
    {
        public random_bytesFunc()
        {
            this.Name = "random_bytes";
            this.MinimumArgCounts = 1;
            this.Run += Random_bytesFunc_Run;
        }

        private void Random_bytesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(RandomNumberGenerator.GetBytes(e.Args[0].AsInt()));
        }
    }

    internal sealed class guid_new_textFunc : FunctionBase
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

    internal sealed class guid_new_bytesFunc : FunctionBase
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