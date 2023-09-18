using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces
{
    //このクラスはデフォルトで読み込まれるため読み込み処理が必要です
    internal sealed class Alice_Initer
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(Core.CoreFunctions));
            //総合関数(コアプロパティ)
            Variable.AddProp(new PropertiesProp());
            Variable.AddProp(new TypProp());
            //統合関数(終わり)
            //複合関数(複数の型に対応する関数)
            Variable.AddProp(new LengthSizeProp(), Constants.LENGTH);
            Variable.AddProp(new LengthSizeProp(), Constants.SIZE);
            Variable.AddProp(new KeysFunc());
            //複合関数(終わり)
            //String関数
            Variable.AddFunc(new str_IsNormalizedFunc());
            //String関数(終わり)
            //DELEGATE系(Delegate.csに本体あり)
            Variable.AddFunc(new DelegateNameFunc());
            //DELEGATE系(終わり)
            Variable.AddFunc(new list_FirstOrLastFunc());
            Variable.AddFunc(new list_FirstOrLastFunc(true));


            NameSpace space = new NameSpace(Constants.TOP_NAMESPACE);
            space.Add(new SingletonFunction());
            space.Add(new IsNaNFunction());
            space.Add(new PrintFunction());
            space.Add(new PrintFunction(true));
            space.Add(new ReadFunction());
            space.Add(new StringFormatFunction());
            space.Add(new ExceptionObject());

            NameSpaceManager.Add(space);

            FunctionBaseManager.Add(new DoWhileStatement());
            FunctionBaseManager.Add(new WhileStatement());
            FunctionBaseManager.Add(new SwitchStatement());
            FunctionBaseManager.Add(new CaseStatement());
            FunctionBaseManager.Add(new CaseStatement(), Constants.DEFAULT);
            FunctionBaseManager.Add(new ForStatement());
            FunctionBaseManager.Add(new ForeachStatement());
            FunctionBaseManager.Add(new GotoGosubFunction(true));
            FunctionBaseManager.Add(new GotoGosubFunction(false));
            FunctionBaseManager.Add(new IncludeFile());
            FunctionBaseManager.Add(new ReturnStatement());
            FunctionBaseManager.Add(new ThrowFunction());
            FunctionBaseManager.Add(new TryBlock());
            FunctionBaseManager.Add(new BlockStatement());

            FunctionBaseManager.Add(new NewObjectFunction());

            FunctionBaseManager.Add(new UsingStatement());
            FunctionBaseManager.Add(new ImportFunc());
            FunctionBaseManager.Add(new DelegateCreator());

        }
    }
    internal sealed class ReturnStatement : FunctionBase
    {
        public ReturnStatement()
        {
            Name = Constants.RETURN;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ReturnStatement_Run;
        }

        private void ReturnStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Script.MoveForwardIf(Constants.SPACE);
            if (!e.Script.FromPrev(Constants.RETURN.Length).Contains(Constants.RETURN))
            {
                e.Script.Backward();
            }
            Variable result = Utils.GetItem(e.Script);

            // Returnに到達したら終了
            e.Script.SetDone();
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            result.IsReturn = true;

            e.Return = result;
        }

    }
    internal sealed class IsNaNFunction : FunctionBase
    {
        public IsNaNFunction()
        {
            Name = Constants.ISNAN;
            MinimumArgCounts = 0;
            Run += IsNaNFunction_Run;
        }

        private void IsNaNFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable arg = e.Args[0];
            e.Return = new Variable(arg.Type != Variable.VarType.NUMBER || double.IsNaN(arg.Value));
        }
    }
    internal sealed class KeysFunc : PropertyBase
    {
        public KeysFunc()
        {
            Name = Constants.KEYS;
            CanSet = false;
            HandleEvents = true;
            Type = Variable.VarType.MAP_NUM | Variable.VarType.MAP_STR;
            Getting += KeysFunc_Getting;

        }

        private void KeysFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetAllKeys());
        }

    }

    internal sealed class PropertiesProp : PropertyBase
    {
        public PropertiesProp()
        {
            Name = Constants.OBJECT_PROPERTIES;
            CanSet = false;
            HandleEvents = true;
            Getting += PropertiesProp_Getting;
        }

        private void PropertiesProp_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetProperties());
        }
    }

    internal sealed class TypProp : PropertyBase
    {
        public TypProp()
        {
            Name = Constants.OBJECT_TYPE;
            CanSet = false;
            HandleEvents = true;
            Getting += TypProp_Getting;
        }

        private void TypProp_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = Variable.AsType(e.Parent.Type);
        }

    }

    internal sealed class LengthSizeProp : PropertyBase
    {
        public LengthSizeProp()
        {
            CanSet = false;
            HandleEvents = true;
            Type = Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY;
            Getting += LengthFunc_Getting;
        }

        private void LengthFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetLength());
        }

    }


    internal sealed class DelegateNameFunc : FunctionBase
    {
        public DelegateNameFunc()
        {
            Name = "Name";
            RequestType = new TypeObject(Variable.VarType.DELEGATE);
            Run += DelegateNameFunc_Run;
        }

        private void DelegateNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                e.Return = new Variable(e.CurentVariable.Delegate.Name);
            }
        }
    }

    //ここより下は変数(Variable)オブジェクトの関数です


    internal sealed class str_IsNormalizedFunc : FunctionBase
    {
        public str_IsNormalizedFunc()
        {
            Name = "IsNormalized";
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_IsNormalizedFunc_Run;
        }

        private void Str_IsNormalizedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().IsNormalized());
        }
    }


    internal sealed class ThrowFunction : FunctionBase
    {
        public ThrowFunction()
        {
            Name = "throw";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            MinimumArgCounts = 1;
            Run += ThrowFunction_Run;
        }

        private void ThrowFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.Args[0].Type)
            {
                case Variable.VarType.STRING:
                    {
                        throw new ScriptException(e.Args[0].AsString(), Exceptions.USER_DEFINED, e.Script);
                    }
                case Variable.VarType.NUMBER:
                    {
                        throw new ScriptException(Utils.GetSafeString(e.Args, 1), (Exceptions)e.Args[0].AsInt(), e.Script);
                    }
                default:
                    {
                        if (e.Args[0].Object is ExceptionObject eo)
                        {
                            var s = eo.MainScript ?? e.Script;
                            throw new ScriptException(eo.Message, eo.ErrorCode, s);
                        }
                        break;
                    }
            }
        }
    }
    internal sealed class GotoGosubFunction : FunctionBase
    {
        private bool m_isGoto = true;

        public GotoGosubFunction(bool gotoMode = true)
        {
            m_isGoto = gotoMode;
            Name = m_isGoto ? Constants.GOTO : Constants.GOSUB;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var labelName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            if (script.AllLabels == null || script.LabelToFile == null |
               !script.AllLabels.TryGetValue(script.FunctionName, out Dictionary<string, int> labels))
            {
                Utils.ThrowErrorMsg("次のラベルは関数内に存在しません [" + script.FunctionName + "]", Exceptions.COULDNT_FIND_LABEL_IN_FUNCTION,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            if (!labels.TryGetValue(labelName, out int gotoPointer))
            {
                Utils.ThrowErrorMsg("ラベル:[" + labelName + "]は定義されていません", Exceptions.COULDNT_FIND_LABEL,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            if (script.LabelToFile.TryGetValue(labelName, out string filename) &&
                filename != script.Filename && !string.IsNullOrWhiteSpace(filename))
            {
                var newScript = script.GetIncludeFileScript(filename, this);
                script.Filename = filename;
                script.String = newScript.String;
            }

            if (!m_isGoto)
            {
                script.PointersBack.Add(script.Pointer);
            }

            script.Pointer = gotoPointer;
            if (string.IsNullOrWhiteSpace(script.FunctionName))
            {
                script.Backward();
            }

            return Variable.EmptyInstance;
        }
    }



    internal sealed class IncludeFile : FunctionBase
    {
        public IncludeFile()
        {
            Name = "include";
            MinimumArgCounts = 1;
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            Run += IncludeFile_Run;
        }

        private void IncludeFile_Run(object sender, FunctionBaseEventArgs e)
        {
            /*
            if (e.Script == null)
            {
                e.Script = new ParsingScript("");
            }
            */
            ParsingScript tempScript = e.Script.GetIncludeFileScript(e.Args[0].AsString(), this);

            Variable result = null;
            while (tempScript.StillValid())
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }
            if (result == null) { result = Variable.EmptyInstance; }
            e.Return = result;
        }

    }

    internal sealed class list_FirstOrLastFunc : FunctionBase
    {
        public list_FirstOrLastFunc(bool isLast = false)
        {
            m_Last = isLast;
            Name = m_Last ? Constants.LAST : Constants.FIRST;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_FirstOrLastFunc_Run;
        }

        private void List_FirstOrLastFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple != null && e.CurentVariable.Tuple.Count > 0)
            {
                e.Return = m_Last ? e.CurentVariable.Tuple[0] : e.CurentVariable.Tuple[e.CurentVariable.Tuple.Count - 1];
            }
        }

        private bool m_Last;
    }


}
