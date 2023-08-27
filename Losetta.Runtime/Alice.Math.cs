namespace AliceScript.NameSpaces
{
    public sealed class Alice_Math
    {
        public static void Init()
        {
            try
            {
                NameSpace space = new NameSpace("Alice.Math");

                space.Add(new math_eFunc());
                space.Add(new math_tauFunc());
                space.Add(new math_piFunc());
                space.Add(new math_absFunc());
                space.Add(new math_acosFunc());
                space.Add(new math_acoshFunc());
                space.Add(new math_atan2Func());
                space.Add(new math_atanFunc());
                space.Add(new math_atanhFunc());
                space.Add(new math_bitdecrementFunc());
                space.Add(new math_bitincrementFunc());
                space.Add(new math_cbrtFunc());
                space.Add(new math_ceilingFunc());
                space.Add(new math_clampFunc());
                space.Add(new math_copysignFunc());
                space.Add(new math_cosFunc());
                space.Add(new math_coshFunc());
                space.Add(new math_expFunc());
                space.Add(new math_floorFunc());
                space.Add(new math_fusedmultiplyaddFunc());
                space.Add(new math_sinFunc());
                space.Add(new math_sinhFunc());
                space.Add(new math_sqrtFunc());
                space.Add(new math_tanFunc());
                space.Add(new math_tanhFunc());
                space.Add(new math_truncateFunc());
                space.Add(new math_isPrimeFunc());
                space.Add(new math_powFunc());
                space.Add(new math_RoundFunc());
                space.Add(new math_MinMaxFunc(true));
                space.Add(new math_MinMaxFunc(false));

                NameSpaceManerger.Add(space);
            }
            catch { }
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

    internal sealed class math_RoundFunc : FunctionBase
    {
        public math_RoundFunc()
        {
            Name = "math_round";
            MinimumArgCounts = 1;
            Run += Math_RoundFunc_Run;
        }

        private void Math_RoundFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = e.Args.Count > 1
                ? new Variable(Math.Round(e.Args[0].AsDouble(), e.Args[1].AsInt()))
                : new Variable(Math.Round(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_powFunc : FunctionBase
    {
        public math_powFunc()
        {
            Name = "math_pow";
            MinimumArgCounts = 2;
            Run += Math_powFunc_Run;
        }

        private void Math_powFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Pow(e.Args[0].Value, e.Args[1].Value));
        }
    }

    internal sealed class math_eFunc : FunctionBase
    {
        public math_eFunc()
        {
            Name = "math_e";
            MinimumArgCounts = 0;
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            Run += Math_EFunc_Run;
        }

        private void Math_EFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.E);
        }
    }

    internal sealed class math_isPrimeFunc : FunctionBase
    {
        public math_isPrimeFunc()
        {
            Name = "math_isPrime";
            MinimumArgCounts = 1;
            Run += Math_isPrimeFunc_Run;
        }

        private void Math_isPrimeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(IsPrime(e.Args[0].AsInt()));
        }

        private bool IsPrime(int num)
        {
            if (num < 2)
            {
                return false;
            }
            else if (num == 2)
            {
                return true;
            }
            else if (num % 2 == 0)
            {
                return false; // 偶数はあらかじめ除く
            }

            double sqrtNum = Math.Sqrt(num);
            for (int i = 3; i <= sqrtNum; i += 2)
            {
                if (num % i == 0)
                {
                    // 素数ではない
                    return false;
                }
            }

            // 素数である
            return true;
        }
    }

    internal sealed class math_tauFunc : FunctionBase
    {
        public math_tauFunc()
        {
            Name = "math_tau";
            MinimumArgCounts = 0;
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            Run += Math_EFunc_Run;
        }

        private void Math_EFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(6.2831853071795862);
        }
    }

    internal sealed class math_piFunc : FunctionBase
    {
        public math_piFunc()
        {
            Name = "math_pi";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            MinimumArgCounts = 0;
            Run += Math_piFunc_Run;
        }

        private void Math_piFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.PI);
        }
    }

    internal sealed class math_absFunc : FunctionBase
    {
        public math_absFunc()
        {
            Name = "math_abs";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Abs(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_acosFunc : FunctionBase
    {
        public math_acosFunc()
        {
            Name = "math_acos";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Acos(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_acoshFunc : FunctionBase
    {
        public math_acoshFunc()
        {
            Name = "math_acosh";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Acosh(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_sinFunc : FunctionBase
    {
        public math_sinFunc()
        {
            Name = "math_sin";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Sin(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_sinhFunc : FunctionBase
    {
        public math_sinhFunc()
        {
            Name = "math_sinh";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Sinh(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_atanFunc : FunctionBase
    {
        public math_atanFunc()
        {
            Name = "math_atan";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Atan(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_atan2Func : FunctionBase
    {
        public math_atan2Func()
        {
            Name = "math_atan2";
            MinimumArgCounts = 2;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Atan2(e.Args[0].AsDouble(), e.Args[1].AsDouble()));
        }
    }

    internal sealed class math_atanhFunc : FunctionBase
    {
        public math_atanhFunc()
        {
            Name = "math_atanh";
            MinimumArgCounts = 1;
            Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Atanh(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_bitdecrementFunc : FunctionBase
    {
        public math_bitdecrementFunc()
        {
            Name = "math_bitdecrement";
            MinimumArgCounts = 1;
            Run += Math_bitdecrementFunc_Run;
        }

        private void Math_bitdecrementFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.BitDecrement(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_bitincrementFunc : FunctionBase
    {
        public math_bitincrementFunc()
        {
            Name = "math_bitincrement";
            MinimumArgCounts = 1;
            Run += Math_bitdecrementFunc_Run;
        }

        private void Math_bitdecrementFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.BitIncrement(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_cbrtFunc : FunctionBase
    {
        public math_cbrtFunc()
        {
            Name = "math_cbrt";
            MinimumArgCounts = 1;
            Run += Math_cbrtFunc_Run;
        }

        private void Math_cbrtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Cbrt(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_ceilingFunc : FunctionBase
    {
        public math_ceilingFunc()
        {
            Name = "math_ceiling";
            MinimumArgCounts = 1;
            Run += Math_cbrtFunc_Run;
        }

        private void Math_cbrtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Ceiling(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_clampFunc : FunctionBase
    {
        public math_clampFunc()
        {
            Name = "math_clamp";
            MinimumArgCounts = 3;
            Run += Math_clampFunc_Run;
        }

        private void Math_clampFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Clamp(e.Args[0].AsDouble(), e.Args[1].AsDouble(), e.Args[2].AsDouble()));
        }
    }

    internal sealed class math_copysignFunc : FunctionBase
    {
        public math_copysignFunc()
        {
            Name = "math_copysign";
            MinimumArgCounts = 2;
            Run += Math_copysignFunc_Run;
        }

        private void Math_copysignFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.CopySign(e.Args[0].AsDouble(), e.Args[1].AsDouble()));
        }

    }

    internal sealed class math_cosFunc : FunctionBase
    {
        public math_cosFunc()
        {
            Name = "math_cos";
            MinimumArgCounts = 1;
            Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Cos(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_coshFunc : FunctionBase
    {
        public math_coshFunc()
        {
            Name = "math_cosh";
            MinimumArgCounts = 1;
            Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Cosh(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_expFunc : FunctionBase
    {
        public math_expFunc()
        {
            Name = "math_exp";
            MinimumArgCounts = 1;
            Run += Math_expFunc_Run;
        }

        private void Math_expFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Exp(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_floorFunc : FunctionBase
    {
        public math_floorFunc()
        {
            Name = "math_floor";
            MinimumArgCounts = 1;
            Run += Math_floorFunc_Run;
        }

        private void Math_floorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Floor(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_fusedmultiplyaddFunc : FunctionBase
    {
        public math_fusedmultiplyaddFunc()
        {
            Name = "math_fusedmultiplyadd";
            MinimumArgCounts = 3;
            Run += Math_fusedmultiplyaddFunc_Run;
        }

        private void Math_fusedmultiplyaddFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.FusedMultiplyAdd(e.Args[0].AsDouble(), e.Args[1].AsDouble(), e.Args[2].AsDouble()));
        }
    }

    internal sealed class math_sqrtFunc : FunctionBase
    {
        public math_sqrtFunc()
        {
            Name = "math_sqrt";
            MinimumArgCounts = 1;
            Run += Math_sqrtFunc_Run;
        }

        private void Math_sqrtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Sqrt(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_tanFunc : FunctionBase
    {
        public math_tanFunc()
        {
            Name = "math_tan";
            MinimumArgCounts = 1;
            Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Tan(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_tanhFunc : FunctionBase
    {
        public math_tanhFunc()
        {
            Name = "math_tanh";
            MinimumArgCounts = 1;
            Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Tanh(e.Args[0].AsDouble()));
        }
    }

    internal sealed class math_truncateFunc : FunctionBase
    {
        public math_truncateFunc()
        {
            Name = "math_truncate";
            MinimumArgCounts = 1;
            Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Truncate(e.Args[0].AsDouble()));
        }
    }


}
