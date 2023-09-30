using AliceScript.Binding;
using AliceScript.Functions;

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
        public static bool Math_IsPrime(long x)
        {
            if (x < 2)
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
            return double.IsFinite(x);
        }
        public static bool Math_IsNormal(double x)
        {
            return double.IsNormal(x);
        }
        public static bool Math_IsSubnormal(double x)
        {
            return double.IsSubnormal(x);
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
            return Math.Clamp(x, min, max);
        }
        public static double Math_CopySign(double x, double y)
        {
            return Math.CopySign(x, y);
        }
        public static double Math_Exp(double x)
        {
            return Math.Exp(x);
        }
        public static double Math_FusedMultiplyAdd(double x, double y, double z)
        {
            return Math.FusedMultiplyAdd(x, y, z);
        }
        public static double Math_Sqrt(double x)
        {
            return Math.Sqrt(x);
        }
        public static double Math_Cbrt(double x)
        {
            return Math.Cbrt(x);
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
        #region 数学定数
        public static double Math_Tau => Math.Tau;
        public static double Math_PI => Math.PI;
        public static double Math_E => Math.E;

        public static double Math_Infinity => double.PositiveInfinity;
        public static double Math_NegativeInfinity => double.NegativeInfinity;

        public static double Math_NaN => double.NaN;

        public static double Math_Epsilon => double.Epsilon;

        public static double Math_MaxValue => double.MaxValue;
        public static double Math_MinValue => double.MinValue;
        #endregion
        #region 端数処理
        public static double Math_Round(double x)
        {
            return Math.Round(x);
        }
        public static double Math_Round(double x, int digits)
        {
            return Math.Round(x, digits);
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
            return Math.BitIncrement(x);
        }
        public static double Math_BitDecrement(double x)
        {
            return Math.BitDecrement(x);
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
            return Math.Asinh(x);
        }
        public static double Math_Acosh(double x)
        {
            return Math.Acosh(x);
        }
        public static double Math_Atanh(double x)
        {
            return Math.Atanh(x);
        }
        #endregion
    }

}
