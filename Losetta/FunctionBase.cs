namespace AliceScript
{
    public class FunctionBase : ParserFunction
    {

        /// <summary>
        /// この関数に必要な引数の最小個数
        /// </summary>
        public int MinimumArgCounts { get; set; }
        /// <summary>
        /// この関数に渡すことができる引数の最大個数
        /// </summary>
        public int MaximumArgCounts { get; set; }

        /// <summary>
        /// この関数の属性を取得または設定します
        /// </summary>
        public FunctionAttribute Attribute
        {
            get => m_Attribute;
            set => m_Attribute = value;
        }
        private FunctionAttribute m_Attribute = FunctionAttribute.GENERAL;

        /// <summary>
        /// この関数が変数のプロパティとして呼び出される場合、その変数の種類を取得または設定します
        /// </summary>
        public TypeObject RequestType { get; set; }

        /// <summary>
        /// この関数が所属する名前空間の名前を取得または設定します。
        /// </summary>
        public string RelatedNameSpace { get; set; }
        public Variable Evaluate(List<Variable> args, ParsingScript script, AliceScriptClass.ClassInstance instance = null)
        {
            FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
            ex.Args = new List<Variable>();
            if (args != null)
            {
                foreach (var a in args)
                {
                    if (a == null)
                    {
                        ex.Args.Add(Variable.EmptyInstance);
                    }
                    else
                    {
                        ex.Args.Add(a);
                    }
                }
            }
            ex.UseObjectResult = false;
            ex.ObjectResult = null;
            if (script != null)
            {
                ex.OriginalScript = script.OriginalScript;
            }
            ex.Return = Variable.EmptyInstance;
            ex.Script = script;
            ex.ClassInstance = instance;
            Run?.Invoke(script, ex);
            if (ex.Return == null)
            {
                ex.Return = Variable.EmptyInstance;
            }
            return ex.UseObjectResult ? new Variable(ex.ObjectResult) : ex.Return;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = null;
            if (!Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE))
            {
                args = ObjectBase.GETTING ? ObjectBase.LaskVariable : script.GetFunctionArgs(this, Constants.START_ARG, Constants.END_ARG);

                if (MinimumArgCounts > 1)
                {
                    Utils.CheckArgs(args.Count, MinimumArgCounts, m_name);
                }
                if (MaximumArgCounts > 0 && args.Count > MaximumArgCounts)
                {
                    throw new ScriptException($"関数 `{m_name}`は、{MaximumArgCounts}個よりも多く引数を持つことができません", Exceptions.TOO_MANY_ARGUREMENTS, script);
                }
            }
            return Evaluate(args, script);
        }
        public Variable Evaluate(ParsingScript script, Variable currentVariable)
        {
            if (currentVariable == null)
            {
                return Variable.EmptyInstance;
            }

            if (RequestType?.Match(currentVariable) == false)
            {
                throw new ScriptException($"関数[{Name}]は無効または定義されていません", Exceptions.COULDNT_FIND_FUNCTION);
            }

            List<Variable> args = GetFunctionArguments(script);

            FunctionBaseEventArgs functionEventArgs = InitializeFunctionEventArgs(script, currentVariable, args);

            Run?.Invoke(script, functionEventArgs);

            return functionEventArgs.UseObjectResult ? new Variable(functionEventArgs.ObjectResult) : functionEventArgs.Return;
        }

        private List<Variable> GetFunctionArguments(ParsingScript script)
        {
            if (Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE))
            {
                return null;
            }

            if (ObjectBase.GETTING)
            {
                return ObjectBase.LaskVariable;
            }

            List<Variable> args = script.GetFunctionArgs(this, Constants.START_ARG, Constants.END_ARG);

            if (MinimumArgCounts >= 1)
            {
                Utils.CheckArgs(args.Count, MinimumArgCounts, Name);
            }

            return args;
        }

        private FunctionBaseEventArgs InitializeFunctionEventArgs(ParsingScript script, Variable currentVariable, List<Variable> args)
        {
            FunctionBaseEventArgs functionEventArgs = new FunctionBaseEventArgs
            {
                Args = args ?? new List<Variable>(),
                UseObjectResult = false,
                ObjectResult = null,
                OriginalScript = script.OriginalScript,
                Return = Variable.EmptyInstance,
                Script = script,
                CurentVariable = currentVariable
            };

            return functionEventArgs;
        }

        public string[] RealArgs { get; internal set; }
        public FunctionBase()
        {
            MinimumArgCounts = 0;
        }
        /// <summary>
        /// この関数が呼び出されたときに発生するイベント
        /// </summary>
        public event FunctionBaseEventHandler Run;
        public Variable GetVaruableFromArgs(List<Variable> args)
        {
            if (MinimumArgCounts >= 1)
            {
                Utils.CheckArgs(args.Count, MinimumArgCounts, m_name);
            }
            FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
            ex.Args = args;
            Run?.Invoke(null, ex);

            return ex.Return;
        }
        public void OnRun(List<Variable> args)
        {
            GetVaruableFromArgs(args);
        }

    }
    /// <summary>
    /// 関数の機能の種類を表します
    /// </summary>
    public enum FunctionAttribute
    {
        /// <summary>
        /// 通常の関数です
        /// </summary>
        GENERAL = 0,
        /// <summary>
        /// 関数の引数に括弧を必要としません（すなわち、空白が使われます）
        /// </summary>
        FUNCT_WITH_SPACE = 1,
        /// <summary>
        /// 関数の引数に括弧を必要としませんが、空白は唯一のものにする必要があります
        /// </summary>
        FUNCT_WITH_SPACE_ONC = 2,
        /// <summary>
        /// フロー関数です。これらの関数の戻り値には意味はありません
        /// </summary>
        CONTROL_FLOW = 3,
        /// <summary>
        /// 言語構造です。これらの関数では引数の自動チェックなどが実行されず、Script以外の要素はすべてNullになります
        /// </summary>
        LANGUAGE_STRUCTURE = 4,
        /// <summary>
        /// オーバーライド可能です。CanOverrideプロパティもしくはこの属性が定義のいずれかが定義されている場合、オーバーライド可能です。
        /// </summary>
        VIRTUAL = 5,
    }

    public static class FunctionBaseManerger
    {
        /// <summary>
        /// 関数をインタプリタに登録し、必要に応じて属性を設定します
        /// </summary>
        /// <param name="func">登録される関数</param>
        /// <param name="name">登録される関数の名前(この項目を省略するとfunc.Nameが使用されます)</param>
        /// <param name="script">登録したいスクリプト(この項目を省略するとグローバルに登録されます)</param>
        /// <param name="isGlobal">常にグローバルに登録する場合はtrue、それ以外の場合はfalse</param>
        public static void Add(FunctionBase func, string name = "", ParsingScript script = null, bool isGlobal = false)
        {

            string fname = func.Name;
            if (!string.IsNullOrEmpty(name))
            {
                fname = name;
            }
            Utils.CheckLegalName(fname);
            if (script == null || isGlobal)
            {
                script = ParsingScript.GetTopLevelScript(script);
            }
            ParserFunction.RegisterScriptFunction(fname, func, script);
            if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
            {
                Constants.FUNCT_WITH_SPACE_ONCE.Add(fname);
            }
            else if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE))
            {
                Constants.FUNCT_WITH_SPACE.Add(fname);
            }
            if (func.Attribute.HasFlag(FunctionAttribute.CONTROL_FLOW))
            {
                Constants.CONTROL_FLOW.Add(fname);
            }
            if (func.Attribute.HasFlag(FunctionAttribute.VIRTUAL))
            {
                func.IsVirtual = true;
            }
        }
        /// <summary>
        /// 関数をインタプリタから登録解除し、必要に応じて属性を解除します
        /// </summary>
        /// <param name="func">登録解除される関数</param>
        /// <param name="name">登録解除される関数の名前(この項目を省略するとfunc.Nameが使用されます)</param>
        /// <param name="script">登録解除される関数の場所(この項目を省略するとグローバルからのみ解除されます)</param>
        public static void Remove(FunctionBase func, string name = "", ParsingScript script = null)
        {
            string fname = name;
            if (!string.IsNullOrEmpty(name))
            {
                fname = name;
            }
            if (script == null)
            {
                ParserFunction.UnregisterFunction(fname);
                if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
                {
                    Constants.FUNCT_WITH_SPACE_ONCE.Remove(fname);
                }
                else if (func.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE))
                {
                    Constants.FUNCT_WITH_SPACE.Remove(fname);
                }
                if (func.Attribute.HasFlag(FunctionAttribute.CONTROL_FLOW))
                {
                    Constants.CONTROL_FLOW.Remove(fname);
                }
            }
            else
            {
                ParserFunction.UnregisterScriptFunction(fname, script);
            }

        }
        /// <summary>
        /// 現在インタプリタに登録されている関数の名前の一覧を取得します
        /// </summary>
        public static List<string> Functions => new List<string>(ParserFunction.s_functions.Keys);
    }
    public delegate void FunctionBaseEventHandler(object sender, FunctionBaseEventArgs e);
    public class FunctionBaseEventArgs : EventArgs
    {
        /// <summary>
        /// 呼び出し元のオリジナルなスクリプトを表します
        /// </summary>
        public string OriginalScript { get; set; }

        /// <summary>
        /// 現在の関数の戻り値を表します
        /// </summary>
        public Variable Return { get; set; }

        /// <summary>
        /// 現在の関数に対しての引数を表します
        /// </summary>
        public List<Variable> Args { get; set; }

        /// <summary>
        /// [使用されていません]
        /// </summary>
        public bool UseObjectResult { get; set; }

        /// <summary>
        /// [使用されていません]
        /// </summary>
        public object ObjectResult { get; set; }

        /// <summary>
        /// 呼び出し内容を含むスクリプト本文を表します
        /// </summary>
        public ParsingScript Script { get; set; }

        /// <summary>
        /// (Variableオブジェクト内のみ)呼び出し元のオブジェクトを表します
        /// </summary>
        public Variable CurentVariable { get; set; }

        public AliceScriptClass.ClassInstance ClassInstance { get; set; }
    }

}
