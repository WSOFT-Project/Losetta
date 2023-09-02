using AliceScript.Interop;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Math
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(MathFunctions));

            NameSpace space = new NameSpace("Alice.Math");
            space.Add(new math_MinMaxFunc(true));
            space.Add(new math_MinMaxFunc(false));
            NameSpaceManager.Add(space);
        }
    }
    internal sealed class math_MinMaxFunc : FunctionBase
    {
        public math_MinMaxFunc(bool max)
        {
            Mode = max;
            Name = Mode ? "math_max" : "math_min";
            MinimumArgCounts = 2;
            Run += Math_MinMaxFunc_Run;
        }

        private void Math_MinMaxFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            double returnValue = 0;
            foreach (Variable v in e.Args)
            {
                if (Mode)
                {
                    if (v.Value > returnValue)
                    {
                        returnValue = v.Value;
                    }
                }
                else
                {
                    if (v.Value < returnValue)
                    {
                        returnValue = v.Value;
                    }
                }
            }
            e.Return = new Variable(returnValue);
        }

        private bool Mode { get; set; }
    }

    [AliceScript.Interop.AliceNameSpace(Name = "Alice.Math")]
    internal static class MathFunctions
    {
        public static double Math_Round(double x)
        {
            return Math.Round(x);
        }
        public static double Math_Round(double x, int digits)
        {
            return Math.Round(x, digits);
        }
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
        public static double Math_Floor(double x)
        {
            return Math.Floor(x);
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
        public static double Math_Truncate(double x)
        {
            return Math.Truncate(x);
        }
        #region 数学定数
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static double Math_Tau()
        {
            return Math.Tau;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static double Math_PI()
        {
            return Math.PI;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static double Math_E()
        {
            return Math.E;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static double Math_Infinity()
        {
            return double.PositiveInfinity;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static double Math_NaN()
        {
            return double.NaN;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static double Math_Epsilon()
        {
            return double.Epsilon;
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
