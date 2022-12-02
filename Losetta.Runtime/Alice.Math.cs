using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript.NameSpaces
{
    static class Alice_Math_Initer
    {
        public static void Init()
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
    }
    class math_MinMaxFunc : FunctionBase
    {
        public math_MinMaxFunc(bool max)
        {
            Mode = max;
            if (Mode)
            {
                this.Name = "math_max";
            }
            else
            {
                this.Name = "math_min";
            }
            this.MinimumArgCounts = 2;
            this.Run += Math_MinMaxFunc_Run;
        }

        private void Math_MinMaxFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            double returnValue = 0;
            foreach(Variable v in e.Args)
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
                    if(v.Value < returnValue)
                    {
                        returnValue = v.Value;
                    }
                }
            }
            e.Return = new Variable(returnValue);
        }

        private bool Mode { get; set; }
    }
    class math_RoundFunc : FunctionBase
    {
        public math_RoundFunc()
        {
            this.Name = "math_round";
            this.MinimumArgCounts = 1;
            this.Run += Math_RoundFunc_Run;
        }

        private void Math_RoundFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                e.Return = new Variable(Math.Round(e.Args[0].AsDouble(),e.Args[1].AsInt()));
            }
            else
            {
                e.Return = new Variable(Math.Round(e.Args[0].AsDouble()));
            }
        }
    }
    class math_powFunc : FunctionBase
    {
        public math_powFunc()
        {
            this.Name = "math_pow";
            this.MinimumArgCounts = 2;
            this.Run += Math_powFunc_Run;
        }

        private void Math_powFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Pow(e.Args[0].Value,e.Args[1].Value));
        }
    }
    class math_eFunc : FunctionBase
    {
        public math_eFunc()
        {
            this.FunctionName = "math_e";
            this.MinimumArgCounts = 0;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.Run += Math_EFunc_Run;
        }

        private void Math_EFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.E);
        }
    }
    class math_isPrimeFunc : FunctionBase
    {
        public math_isPrimeFunc()
        {
            this.Name = "math_isPrime";
            this.MinimumArgCounts = 1;
            this.Run += Math_isPrimeFunc_Run;
        }

        private void Math_isPrimeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(IsPrime(e.Args[0].AsInt()));
        }
        bool IsPrime(int num)
        {
            if (num < 2) return false;
            else if (num == 2) return true;
            else if (num % 2 == 0) return false; // 偶数はあらかじめ除く

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
    class math_tauFunc : FunctionBase
    {
        public math_tauFunc()
        {
            this.FunctionName = "math_tau";
            this.MinimumArgCounts = 0;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.Run += Math_EFunc_Run;
        }

        private void Math_EFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(6.2831853071795862);
        }
    }
    class math_piFunc : FunctionBase
    {
        public math_piFunc()
        {
            this.Name = "math_pi";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 0;
            this.Run += Math_piFunc_Run;
        }

        private void Math_piFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.PI);
        }
    }
    class math_absFunc : FunctionBase
    {
        public math_absFunc()
        {
            this.FunctionName = "math_abs";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Abs(e.Args[0].AsDouble()));
        }
    }
    class math_acosFunc : FunctionBase
    {
        public math_acosFunc()
        {
            this.FunctionName = "math_acos";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Acos(e.Args[0].AsDouble()));
        }
    }
    class math_acoshFunc : FunctionBase
    {
        public math_acoshFunc()
        {
            this.FunctionName = "math_acosh";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Acosh(e.Args[0].AsDouble()));
        }
    }
    class math_sinFunc : FunctionBase
    {
        public math_sinFunc()
        {
            this.FunctionName = "math_sin";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Sin(e.Args[0].AsDouble()));
        }
    }
    class math_sinhFunc : FunctionBase
    {
        public math_sinhFunc()
        {
            this.FunctionName = "math_sinh";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Sinh(e.Args[0].AsDouble()));
        }
    }
    class math_atanFunc : FunctionBase
    {
        public math_atanFunc()
        {
            this.FunctionName = "math_atan";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Atan(e.Args[0].AsDouble()));
        }
    }
    class math_atan2Func : FunctionBase
    {
        public math_atan2Func()
        {
            this.FunctionName = "math_atan2";
            this.MinimumArgCounts = 2;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Atan2(e.Args[0].AsDouble(),e.Args[1].AsDouble()));
        }
    }
    class math_atanhFunc : FunctionBase
    {
        public math_atanhFunc()
        {
            this.FunctionName = "math_atanh";
            this.MinimumArgCounts = 1;
            this.Run += Math_absFunc_Run;
        }

        private void Math_absFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Atanh(e.Args[0].AsDouble()));
        }
    }
    class math_bitdecrementFunc : FunctionBase
    {
        public math_bitdecrementFunc()
        {
            this.FunctionName = "math_bitdecrement";
            this.MinimumArgCounts = 1;
            this.Run += Math_bitdecrementFunc_Run;
        }

        private void Math_bitdecrementFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.BitDecrement(e.Args[0].AsDouble()));
        }
    }
    class math_bitincrementFunc : FunctionBase
    {
        public math_bitincrementFunc()
        {
            this.FunctionName = "math_bitincrement";
            this.MinimumArgCounts = 1;
            this.Run += Math_bitdecrementFunc_Run;
        }

        private void Math_bitdecrementFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.BitIncrement(e.Args[0].AsDouble()));
        }
    }
    class math_cbrtFunc : FunctionBase
    {
        public math_cbrtFunc()
        {
            this.FunctionName = "math_cbrt";
            this.MinimumArgCounts = 1;
            this.Run += Math_cbrtFunc_Run;
        }

        private void Math_cbrtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Cbrt(e.Args[0].AsDouble()));
        }
    }
    class math_ceilingFunc : FunctionBase
    {
        public math_ceilingFunc()
        {
            this.FunctionName = "math_ceiling";
            this.MinimumArgCounts = 1;
            this.Run += Math_cbrtFunc_Run;
        }

        private void Math_cbrtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Ceiling(e.Args[0].AsDouble()));
        }
    }
    class math_clampFunc : FunctionBase
    {
        public math_clampFunc()
        {
            this.FunctionName = "math_clamp";
            this.MinimumArgCounts = 3;
            this.Run += Math_clampFunc_Run;
        }

        private void Math_clampFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Clamp(e.Args[0].AsDouble(),e.Args[1].AsDouble(),e.Args[2].AsDouble()));
        }
    }
    class math_copysignFunc : FunctionBase
    {
        public math_copysignFunc()
        {
            this.FunctionName = "math_copysign";
            this.MinimumArgCounts = 2;
            this.Run += Math_copysignFunc_Run;
        }

        private void Math_copysignFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.CopySign(e.Args[0].AsDouble(),e.Args[1].AsDouble()));
        }
    
    }
    class math_cosFunc : FunctionBase
    {
        public math_cosFunc()
        {
            this.FunctionName = "math_cos";
            this.MinimumArgCounts = 1;
            this.Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Cos(e.Args[0].AsDouble()));
        }
    }
    class math_coshFunc : FunctionBase
    {
        public math_coshFunc()
        {
            this.FunctionName = "math_cosh";
            this.MinimumArgCounts = 1;
            this.Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Cosh(e.Args[0].AsDouble()));
        }
    }
    class math_expFunc : FunctionBase
    {
        public math_expFunc()
        {
            this.FunctionName = "math_exp";
            this.MinimumArgCounts = 1;
            this.Run += Math_expFunc_Run;
        }

        private void Math_expFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Exp(e.Args[0].AsDouble()));
        }
    }
    class math_floorFunc : FunctionBase
    {
        public math_floorFunc()
        {
            this.FunctionName = "math_floor";
            this.MinimumArgCounts = 1;
            this.Run += Math_floorFunc_Run;
        }

        private void Math_floorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Floor(e.Args[0].AsDouble()));
        }
    }
    class math_fusedmultiplyaddFunc : FunctionBase
    {
        public math_fusedmultiplyaddFunc()
        {
            this.Name = "math_fusedmultiplyadd";
            this.MinimumArgCounts = 3;
            this.Run += Math_fusedmultiplyaddFunc_Run;
        }

        private void Math_fusedmultiplyaddFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.FusedMultiplyAdd(e.Args[0].AsDouble(),e.Args[1].AsDouble(),e.Args[2].AsDouble()));
        }
    }
    class math_sqrtFunc : FunctionBase
    {
        public math_sqrtFunc()
        {
            this.Name = "math_sqrt";
            this.MinimumArgCounts = 1;
            this.Run += Math_sqrtFunc_Run;
        }

        private void Math_sqrtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Sqrt(e.Args[0].AsDouble()));
        }
    }
    class math_tanFunc : FunctionBase
    {
        public math_tanFunc()
        {
            this.FunctionName = "math_tan";
            this.MinimumArgCounts = 1;
            this.Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Tan(e.Args[0].AsDouble()));
        }
    }
    class math_tanhFunc : FunctionBase
    {
        public math_tanhFunc()
        {
            this.FunctionName = "math_tanh";
            this.MinimumArgCounts = 1;
            this.Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Tanh(e.Args[0].AsDouble()));
        }
    }
    class math_truncateFunc : FunctionBase
    {
        public math_truncateFunc()
        {
            this.FunctionName = "math_truncate";
            this.MinimumArgCounts = 1;
            this.Run += Math_cosFunc_Run;
        }

        private void Math_cosFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Math.Truncate(e.Args[0].AsDouble()));
        }
    }
  

}
