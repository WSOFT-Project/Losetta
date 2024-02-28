using AliceScript.Binding;
using System;
using System.Security.Cryptography;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Random
    {
        public static void Init()
        {
            Alice.RegisterFunctions<RandomFunctions>();
        }
    }
    [AliceNameSpace(Name = "Alice.Random")]
    internal sealed class RandomFunctions
    {
        #region 乱数生成
        private static Random random = new Random();
        public static int Rand()
        {
            return random.Next();
        }
        public static int Rand(int max)
        {
            return random.Next(max);
        }
        public static int Rand(int min, int max)
        {
            return random.Next(min, max);
        }
        public static byte[] Rand_Bytes(int length)
        {
            byte[] bs = new byte[length];
            random.NextBytes(bs);
            return bs;
        }
        public static double Rand_Double()
        {
            return random.NextDouble();
        }
        #endregion
        #region 暗号学的乱数生成
        public static int Random_Int()
        {
#if NETCOREAPP3_0_OR_GREATER
            return RandomNumberGenerator.GetInt32(int.MaxValue);
#else
            throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static int Random_Int(int max)
        {
#if NETCOREAPP3_0_OR_GREATER
            return RandomNumberGenerator.GetInt32(max);
#else
            throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static int Random_Int(int min, int max)
        {
#if NETCOREAPP3_0_OR_GREATER
            return RandomNumberGenerator.GetInt32(min, max);
#else
            throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static byte[] Random_Bytes(int length)
        {
            return RandomNumberGenerator.GetBytes(length);
        }
#endregion
        #region GUID生成
        public static string Guid_New_Text()
        {
            return Guid.NewGuid().ToString();
        }
        public static byte[] Guid_New_Bytes()
        {
            return Guid.NewGuid().ToByteArray();
        }
        #endregion
    }
}