using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.Functions
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

        /// <summary>
        /// この関数を拡張メソッドとして呼び出し可能な場合はTrue、それ以外の場合はfalse。このプロパティは読み取り専用です。
        /// </summary>
        public bool IsMethod => RequestType is not null;

        /// <summary>
        /// この関数が拡張メソッドとして使用可能なとき、この関数は拡張メソッドとしてのみ呼び出すことができる
        /// </summary>
        public bool MethodOnly { get; set; } = true;
        /// <summary>
        /// この関数を呼び出します
        /// </summary>
        /// <param name="args">呼び出しに使用する引数</param>
        /// <param name="script">呼び出し元のスクリプト</param>
        /// <param name="instance">呼び出し元のクラスインスタンス</param>
        /// <returns>この関数の戻り値</returns>
        public Variable Evaluate(List<Variable> args, ParsingScript script, AliceScriptClass.ClassInstance instance = null)
        {
            FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
            ex.Args = args ?? new List<Variable>();
            ex.UseObjectResult = false;
            ex.ObjectResult = null;
            if (script is not null)
            {
                ex.OriginalScript = script.OriginalScript;
            }
            ex.Return = Variable.EmptyInstance;
            ex.Script = script;
            ex.Keywords = Keywords;
            ex.ClassInstance = instance;
            Run?.Invoke(script, ex);
            if (ex.Return is null)
            {
                ex.Return = Variable.EmptyInstance;
            }
            return ex.UseObjectResult ? new Variable(ex.ObjectResult) : ex.Return;
        }

        /// <summary>
        /// この関数を呼び出し、結果を取得します
        /// </summary>
        /// <param name="script">呼び出し元のスクリプト</param>
        /// <returns>この関数の戻り値</returns>
        /// <exception cref="ScriptException">受け入れ範囲外の引数を受取ることはできません</exception>
        public Variable Execute(ParsingScript script)
        {
            return Evaluate(script);
        }
        /// <summary>
        /// この関数を呼び出します
        /// </summary>
        /// <param name="script">呼び出し元のスクリプト</param>
        /// <returns>この関数の戻り値</returns>
        /// <exception cref="ScriptException">受け入れ範囲外の引数を受取ることはできません</exception>
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
        /// <summary>
        /// この拡張メソッドを呼び出します
        /// </summary>
        /// <param name="script">呼び出し元のスクリプト</param>
        /// <param name="currentVariable">呼び出し元の変数</param>
        /// <returns>この拡張メソッドの戻り値</returns>
        /// <exception cref="ScriptException">この拡張メソッドが使用できない場合はエラー</exception>
        public Variable Evaluate(ParsingScript script, Variable currentVariable)
        {
            if (currentVariable is null)
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
            if (Attribute == FunctionAttribute.LANGUAGE_STRUCTURE)
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
        public AccessModifier AccessModifier { get; set; }
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

    }
    public class AttributeFunction : FunctionBase
    {

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
        VIRTUAL = 5
    }
    /// <summary>
    /// 関数に使用されるアクセス修飾子を表します
    /// </summary>
    public enum AccessModifier
    {
        /// <summary>
        /// スコープの範囲内からのみアクセスできます
        /// </summary>
        PRIVATE = 0,
        /// <summary>
        /// 名前空間の外部からもアクセスできます
        /// </summary>
        PUBLIC = 1,
        /// <summary>
        /// 名前空間の内部からのみアクセスできます
        /// </summary>
        INTERNAL = 2
    }
    /// <summary>
    /// 関数を登録または登録解除する操作を提供します
    /// </summary>
    public static class FunctionBaseManager
    {
        /// <summary>
        /// 関数をインタプリタに登録し、必要に応じて属性を設定します
        /// </summary>
        /// <param name="func">登録される関数</param>
        /// <param name="name">登録される関数の名前(この項目を省略するとfunc.Nameが使用されます)</param>
        /// <param name="script">登録したいスクリプト(この項目を省略するとグローバルに登録されます)</param>
        /// <param name="accessModifier">関数のアクセス修飾子</param>
        /// <param name="byPassCheck">識別子のチェックをバイパスする場合はtrue、それ以外の場合はfalse</param>
        public static void Add(FunctionBase func, string name = "", ParsingScript script = null, AccessModifier accessModifier = AccessModifier.PRIVATE, bool byPassCheck = false)
        {
            string fname = func.Name;
            func.AccessModifier = accessModifier;
            if (!string.IsNullOrEmpty(name))
            {
                fname = name;
            }
            if (!byPassCheck)
            {
                Utils.CheckLegalName(fname);
            }
            script ??= ParsingScript.GetTopLevelScript(script);
            if (accessModifier == AccessModifier.PRIVATE)
            {
                ParserFunction.RegisterScriptFunction(fname, func, script);
            }
            else
            {
                script.NameSpace.Add(func, true, accessModifier);
            }
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
            if (script is null)
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

        /// <summary>
        /// この関数の呼び出し時に同時に指定されたキーワードを表します
        /// </summary>
        public HashSet<string> Keywords { get; set; }

        public AliceScriptClass.ClassInstance ClassInstance { get; set; }
    }

}
