using AliceScript.Binding;
using System;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Math
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(MathFunctions));
        }
    }

    [AliceNameSpace(Name = "Alice.Math")]
    internal static class MathFunctions
    {
        public static bool Math_IsPrime(double x)
        {
            if (x < 2 || double.IsNaN(x) || double.IsInfinity(x))
            {
                return false;
            }
            else if (x == 2)
            {
                return true;
            }
            else if (x % 2 == 0)
            {
                return false; // 偶数はあらかじめ除く
            }

            double sqrtNum = Math.Sqrt(x);
            for (int i = 3; i <= sqrtNum; i += 2)
            {
                if (x % i == 0)
                {
                    // 素数ではない
                    return false;
                }
            }

            // 素数である
            return true;
        }
        public static bool Math_IsNaN(double x)
        {
            return double.IsNaN(x);
        }

        public static bool Math_IsInfinity(double x)
        {
            return double.IsInfinity(x);
        }

        public static bool Math_IsPositiveInfinity(double x)
        {
            return double.IsPositiveInfinity(x);
        }

        public static bool Math_IsNegativeInfinity(double x)
        {
            return double.IsNegativeInfinity(x);
        }
        public static bool Math_IsFinite(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return double.IsFinite(x);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static bool Math_IsNormal(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return double.IsNormal(x);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static bool Math_IsSubnormal(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return double.IsSubnormal(x);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static double Math_Pow(double x, double y)
        {
            return Math.Pow(x, y);
        }
        public static double Math_Abs(double x)
        {
            return Math.Abs(x);
        }
        public static double Math_Ceiling(double x)
        {
            return Math.Ceiling(x);
        }
        public static double Math_Clamp(double x, double min, double max)
        {
            return double.IsNaN(x) ? double.NaN : x < min ? min : max < x ? max : x;
        }
        public static double Math_CopySign(double x, double y)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Math.CopySign(x, y);

#else
                int sign = Math.Sign(y);
                sign = sign == 0 ? 1 : sign;
                return Math.Abs(x) * sign;
#endif
        }
        public static double Math_Exp(double x)
        {
            return Math.Exp(x);
        }
        public static double Math_FusedMultiplyAdd(double x, double y, double z)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Math.FusedMultiplyAdd(x, y, z);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static double Math_Sqrt(double x)
        {
            return Math.Sqrt(x);
        }
        public static double Math_Cbrt(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return Math.Cbrt(x);
#else
            return Math.Pow(x,1/3);
#endif
        }
        public static double Math_Max(params double[] nums)
        {
            double max = double.MinValue;
            foreach (double d in nums)
            {
                max = Math.Max(max, d);
            }
            return max;
        }
        public static double Math_Min(params double[] nums)
        {
            double min = double.MaxValue;
            foreach (double d in nums)
            {
                min = Math.Min(min, d);
            }
            return min;
        }
        public static double Math_Factorial(uint n)
        {
            // これ以上の階乗は正の無限を返す
            const int MAX_INPUT = 170;

            if (n > MAX_INPUT)
            {
                return double.PositiveInfinity;
            }

            double factorial = 1;

            for (int i = 1; i <= n; i++)
            {
                factorial *= i;
            }

            return factorial;
        }

        #region 数学定数
        public static double Math_Tau => 6.2831853071795862;
        public static double Math_PI => 3.1415926535897931;
        public static double Math_E => 2.7182818284590451;

        public static double Math_Infinity => double.PositiveInfinity;
        public static double Math_NegativeInfinity => double.NegativeInfinity;

        public static double Math_NaN => double.NaN;

        public static double Math_Epsilon => double.Epsilon;

        public static double Math_MaxValue => double.MaxValue;
        public static double Math_MinValue => double.MinValue;
        #endregion
        #region 端数処理
        public static double Math_Round(double x, bool? roudingMode = null)
        {
#if NETCOREAPP3_0_OR_GREATER
            MidpointRounding mode = roudingMode.HasValue ? roudingMode.Value ? MidpointRounding.AwayFromZero : MidpointRounding.ToZero : MidpointRounding.ToEven;
#else
            MidpointRounding mode = roudingMode.HasValue ? roudingMode.Value ? MidpointRounding.AwayFromZero :  throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED) : MidpointRounding.ToEven;
#endif
            return Math.Round(x, mode);
        }
        public static double Math_Round(double x, int digits, bool? roudingMode = null)
        {
#if NETCOREAPP3_0_OR_GREATER
            MidpointRounding mode = roudingMode.HasValue ? roudingMode.Value ? MidpointRounding.AwayFromZero : MidpointRounding.ToZero : MidpointRounding.ToEven;
#else
            MidpointRounding mode = roudingMode.HasValue ? roudingMode.Value ? MidpointRounding.AwayFromZero :  throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED) : MidpointRounding.ToEven;
#endif
            return Math.Round(x, digits, mode);
        }
        public static double Math_Truncate(double x)
        {
            return Math.Truncate(x);
        }

        public static double Math_Floor(double x)
        {
            return Math.Floor(x);
        }
        #endregion
        #region ビット加減算
        public static double Math_BitIncrement(double x)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Math.BitIncrement(x);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static double Math_BitDecrement(double x)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Math.BitDecrement(x);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        #endregion
        #region 三角関数
        public static double Math_sin(double x)
        {
            return Math.Sin(x);
        }

        public static double Math_cos(double x)
        {
            return Math.Cos(x);
        }

        public static double Math_tan(double x)
        {
            return Math.Tan(x);
        }
        #endregion
        #region 逆三角関数
        public static double Math_Asin(double x)
        {
            return Math.Asin(x);
        }
        public static double Math_Acos(double x)
        {
            return Math.Acos(x);
        }
        public static double Math_Atan(double x)
        {
            return Math.Atan(x);
        }
        public static double Math_Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }
        #endregion
        #region 双曲線関数
        public static double Math_Sinh(double x)
        {
            return Math.Sinh(x);
        }

        public static double Math_Consh(double x)
        {
            return Math.Cosh(x);
        }

        public static double Math_Tanh(double x)
        {
            return Math.Tanh(x);
        }
        #endregion
        #region 逆双曲線関数
        public static double Math_Asinh(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return Math.Asinh(x);
#else
            return Math.Log(x + Math.Sqrt(x * x + 1));
#endif
        }
        public static double Math_Acosh(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return Math.Acosh(x);
#else
            return x < 1.0 || double.IsNaN(x)
                ? double.NaN
                : Math.Log(x + Math.Sqrt((x * x) - 1));
#endif
        }
        public static double Math_Atanh(double x)
        {
#if NETCOREAPP2_1_OR_GREATER
            return Math.Atanh(x);
#else
            if(Math.Abs(x) > 1 || double.IsNaN(x))
            {
                return double.NaN;
            }
            return 0.5 * Math.Log((1 + x) / (1 - x));
#endif
        }
        #endregion
        #region 対数関数
        public static double Math_Log(double a)
        {
            return Math.Log(a);
        }

        public static double Math_Log(double a, double baseNum)
        {
            return Math.Log(a, baseNum);
        }
        #endregion


        public static double Math_ReciprocalEstimate(double a)
        {
#if NET6_0_OR_GREATER
            return Math.ReciprocalEstimate(a);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
    }

}
