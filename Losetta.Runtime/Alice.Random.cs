using System.Security.Cryptography;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Random
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


                NameSpaceManager.Add(space);
            }
            catch { }
        }
    }

    internal sealed class randFunc : FunctionBase
    {
        public randFunc()
        {
            Name = "rand";
            MinimumArgCounts = 0;
            Run += RandFunc_Run;
        }

        private void RandFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                e.Return = new Variable(Alice_Random.random.Next());
            }
            else if (e.Args.Count == 1)
            {
                e.Return = new Variable(Alice_Random.random.Next(e.Args[0].AsInt()));
            }
            else if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Random.random.Next(e.Args[0].AsInt(), e.Args[1].AsInt()));
            }
        }
    }

    internal sealed class rand_bytesFunc : FunctionBase
    {
        public rand_bytesFunc()
        {
            Name = "rand_bytes";
            MinimumArgCounts = 1;
            Run += RandFunc_Run;
        }

        private void RandFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] bs = new byte[e.Args[0].AsInt()];
            Alice_Random.random.NextBytes(bs);
            e.Return = new Variable(bs);
        }
    }

    internal sealed class rand_doubleFunc : FunctionBase
    {
        public rand_doubleFunc()
        {
            Name = "rand_double";
            MinimumArgCounts = 0;
            Run += RandFunc_Run;
        }

        private void RandFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice_Random.random.NextDouble());
        }
    }

    internal sealed class random_intFunc : FunctionBase
    {
        public random_intFunc()
        {
            Name = "random_int";
            MinimumArgCounts = 0;
            Run += Random_intFunc_Run;
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
            Name = "random_bytes";
            MinimumArgCounts = 1;
            Run += Random_bytesFunc_Run;
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
            Name = "guid_new_text";
            Run += GuidFunc_Run;
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
            Name = "guid_new_bytes";
            Run += GuidFunc_Run;
        }

        private void GuidFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Guid.NewGuid().ToByteArray());
        }
    }
}