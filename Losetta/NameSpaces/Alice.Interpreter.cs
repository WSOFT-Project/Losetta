using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Objects;
using AliceScript.Packaging;
using AliceScript.Parsing;

namespace AliceScript
{
    internal static class Alice_Interpreter_Initer
    {
        public static void Init()
        {
            try
            {
                NameSpace space = new NameSpace("Alice.Interpreter");

                space.Add(new Interpreter_Reset_VariablesFunc());
                space.Add(new Interpreter_Append_OutputOrDataFunc());
                space.Add(new Interpreter_GetParentFunc());
                space.Add(new Interpreter_Append_OutputOrDataFunc(true));
                space.Add(new Interpreter_NameExistsFunc());
                space.Add(new Interpreter_ProcessOrFileFunc());
                space.Add(new Interpreter_ProcessOrFileFunc(true));
                space.Add(new Interpreter_GetVariable());
                space.Add(new Interpreter_NamespacesFunc());
                space.Add(new Interpreter_FunctionsFunc());
                space.Add(new Interpreter_VariablesFunc());
                space.Add(new Interpreter_NameFunc());
                space.Add(new GetPackageFunc());
                space.Add(new Interpreter_ConstsFunc());
                space.Add(new Interpreter_ScriptObject(null));
                space.Add(new Interpreter_GetScriptFunc());
                space.Add(new gc_collectFunc());
                space.Add(new gc_gettotalmemoryFunc());
                space.Add(new Bind_RegisterFunc());
                space.Add(new TypeObject());

                NameSpaceManager.Add(space);
            }
            catch { }
        }
    }
    internal class Bind_RegisterFunc : FunctionBase
    {
        public Bind_RegisterFunc()
        {
            Name = "Bind_Register";
            MinimumArgCounts = 1;
            Run += Bind_Register_Run;
        }

        private void Bind_Register_Run(object sender, FunctionBaseEventArgs e)
        {
            Type t = Type.GetType(e.Args[0].ToString());
            if (t == null)
            {
                throw new ScriptException($"{e.Args[0]}という名前の型を検索できませんでした。アセンブリ名の指定を忘れていませんか？", Exceptions.OBJECT_DOESNT_EXIST);
            }
            NameSpaceManager.Add(t);
        }
    }
    internal class Interpreter_GetParentFunc : FunctionBase
    {
        public Interpreter_GetParentFunc()
        {
            Name = "Interpreter_GetParent";
            MinimumArgCounts = 1;
            Run += Interpreter_GetParentFunc_Run;
        }

        private void Interpreter_GetParentFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type == Variable.VarType.OBJECT && e.Args[0].Object is Interpreter_ScriptObject o)
            {
                e.Return = new Variable(new Interpreter_ScriptObject(o.Script.ParentScript));
            }
            else if (e.Args[0].Parent != null)
            {
                e.Return = new Variable(new Interpreter_ScriptObject(e.Args[0].Parent));
            }
        }
    }
    internal class Interpreter_NameExistsFunc : FunctionBase
    {
        public Interpreter_NameExistsFunc()
        {
            Name = "Interpreter_NameExists";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += Interpreter_NameExistsFunc_Run;
        }

        private void Interpreter_NameExistsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string varName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            varName = Constants.ConvertName(varName);

            bool result = ParserFunction.GetVariable(varName, e.Script) != null;
            e.Return = new Variable(result);
        }
    }


    internal class gc_collectFunc : FunctionBase
    {
        public gc_collectFunc()
        {
            Name = "gc_collect";
            MinimumArgCounts = 0;
            Run += Gc_collectFunc_Run;
        }

        private void Gc_collectFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            GC.Collect();
        }
    }

    internal class gc_gettotalmemoryFunc : FunctionBase
    {
        public gc_gettotalmemoryFunc()
        {
            Name = "gc_gettotalmemory";
            MinimumArgCounts = 1;
            Run += Gc_gettotalmemoryFunc_Run;
        }

        private void Gc_gettotalmemoryFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(GC.GetTotalMemory(e.Args[0].AsBool()));
        }
    }


    internal class Interpreter_Reset_VariablesFunc : FunctionBase
    {
        public Interpreter_Reset_VariablesFunc()
        {
            Name = "Interpreter_Reset_Variables";
            Run += Interpreter_Reset_VariablesFunc_Run;
        }

        private void Interpreter_Reset_VariablesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            ParserFunction.CleanUpVariables();
        }
    }

    internal class Interpreter_Append_OutputOrDataFunc : FunctionBase
    {
        public Interpreter_Append_OutputOrDataFunc(bool isdata = false)
        {
            m_isData = isdata;
            Name = isdata ? "Interpreter_Append_Data" : "Interpreter_Append_Output";
            MinimumArgCounts = 1;
            Run += Interpreter_Append_OutputOrDataFunc_Run;
        }

        private void Interpreter_Append_OutputOrDataFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_isData)
            {
                e.Return = new Variable(Interpreter.Instance.AppendData(e.Args[0].AsString(), Utils.GetSafeBool(e.Args, 1)));
            }
            else
            {
                Interpreter.Instance.AppendOutput(e.Args[0].AsString(), Utils.GetSafeBool(e.Args, 1));
            }
        }

        private bool m_isData = false;
    }

    internal class Interpreter_ProcessOrFileFunc : FunctionBase
    {
        public Interpreter_ProcessOrFileFunc(bool isfile = false)
        {
            m_isFile = isfile;
            Name = m_isFile ? "Interpreter_ProcessFile" : "Interpreter_Process";
            MinimumArgCounts = 1;
            Run += Interpreter_ProcessOrFileFunc_Run;
        }

        private void Interpreter_ProcessOrFileFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = m_isFile
                ? Interpreter.Instance.ProcessFile(e.Args[0].AsString(), Utils.GetSafeBool(e.Args, 1))
                : Interpreter.Instance.Process(e.Args[0].AsString(), Utils.GetSafeString(e.Args, 1), Utils.GetSafeBool(e.Args, 2));
        }

        private bool m_isFile = false;
    }

    internal class Interpreter_FunctionsFunc : FunctionBase
    {
        public Interpreter_FunctionsFunc()
        {
            Name = "Interpreter_Functions";
            Run += FunctionsFunc_Run;
        }

        private void FunctionsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                Variable v = new Variable(Variable.VarType.ARRAY);
                foreach (string s in FunctionBaseManager.Functions)
                {
                    v.Tuple.Add(new Variable(s));
                }
                e.Return = v;
            }
            else
            {
                string str = Utils.GetSafeString(e.Args, 0);
                if (NameSpaceManager.Contains(str))
                {
                    Variable v = new Variable(Variable.VarType.ARRAY);
                    foreach (FunctionBase fb in NameSpaceManager.NameSpaces[str].Functions)
                    {
                        v.Tuple.Add(new Variable(fb.Name));
                    }
                    e.Return = v;
                }
                else
                {
                    throw new ScriptException("指定された名前空間が見つかりませんでした", Exceptions.NAMESPACE_NOT_FOUND);
                }
            }
        }
    }

    internal class Interpreter_NamespacesFunc : FunctionBase
    {
        public Interpreter_NamespacesFunc()
        {
            Name = "Interpreter_Namespaces";
            Run += NamespacesFunc_Run;
        }

        private void NamespacesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach (string s in NameSpaceManager.NameSpaces.Keys)
            {
                v.Tuple.Add(new Variable(s));
            }
            e.Return = v;
        }
    }

    internal class Interpreter_VariablesFunc : FunctionBase
    {
        public Interpreter_VariablesFunc()
        {
            Name = "Interpreter_GlobalVariables";
            Run += Interpreter_VariablesFunc_Run;
        }

        private void Interpreter_VariablesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach (string s in ParserFunction.s_variables.Keys)
            {
                v.Tuple.Add(new Variable(s));
            }
            e.Return = v;
        }
    }

    internal class Interpreter_GetVariable : FunctionBase
    {
        public Interpreter_GetVariable()
        {
            Name = "Interpreter_GetVariable";
            MinimumArgCounts = 1;
            Run += Interpreter_GetVariable_Run;
        }

        private void Interpreter_GetVariable_Run(object sender, FunctionBaseEventArgs e)
        {
            string name = e.Args[0].AsString();
            e.Return = (e.Script.TryGetVariable(name, out ParserFunction impl) || ParserFunction.s_functions.TryGetValue(name, out impl)) && impl is GetVarFunction vf
                ? vf.Value
                : throw new ScriptException("指定された名前の変数は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, e.Script);
        }
    }



    internal class Interpreter_GetScriptFunc : FunctionBase
    {
        public Interpreter_GetScriptFunc()
        {
            Name = "Interpreter_GetScript";
            Run += Interpreter_GetScriptFunc_Run;
        }

        private void Interpreter_GetScriptFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(new Interpreter_ScriptObject(e.Script));
        }
    }

    internal class Interpreter_ConstsFunc : FunctionBase
    {
        public Interpreter_ConstsFunc()
        {
            Name = "Interpreter_Consts";
            MinimumArgCounts = 0;
            Attribute = FunctionAttribute.GENERAL;
            Run += Interpreter_ConstsFunc_Run;
        }

        private void Interpreter_ConstsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach (string s in Constants.CONSTS.Keys)
            {
                v.Tuple.Add(Variable.FromText(s));
            }
            e.Return = v;
        }
    }
    internal class GetPackageFunc : FunctionBase
    {
        public GetPackageFunc()
        {
            Name = "Interpreter_GetPackage";
            Run += GetPackageFunc_Run;
        }

        private void GetPackageFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Script.Package != null)
            {
                e.Return = new Variable(new AlicePackageObject(e.Script.Package));
            }
        }
        internal class AlicePackageObject : ObjectBase
        {
            public AlicePackageObject(AlicePackage package)
            {
                Name = "AlicePackage";
                Package = package;
                AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
                AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
                AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
                AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));

                AddFunction(new AlicePackageObject_EntryIOFunctions(this, AlicePackageObject_EntryIOFunctions.AlicePackageObjectt_EntryIOFunctionMode.ReadData));
                AddFunction(new AlicePackageObject_EntryIOFunctions(this, AlicePackageObject_EntryIOFunctions.AlicePackageObjectt_EntryIOFunctionMode.ReadText));
                AddFunction(new AlicePackageObject_EntryIOFunctions(this, AlicePackageObject_EntryIOFunctions.AlicePackageObjectt_EntryIOFunctionMode.Exists));
            }
            public AlicePackage Package { get; set; }
            private class AlicePackageObject_EntryIOFunctions : FunctionBase
            {
                public AlicePackageObject_EntryIOFunctions(AlicePackageObject package, AlicePackageObjectt_EntryIOFunctionMode mode)
                {
                    switch (mode)
                    {
                        case AlicePackageObjectt_EntryIOFunctionMode.Exists:
                            {
                                Name = "entry_exists";
                                MinimumArgCounts = 1;
                                break;
                            }
                        case AlicePackageObjectt_EntryIOFunctionMode.ReadData:
                            {
                                Name = "entry_read_data";
                                MinimumArgCounts = 1;
                                break;
                            }
                        case AlicePackageObjectt_EntryIOFunctionMode.ReadText:
                            {
                                Name = "entry_read_text";
                                MinimumArgCounts = 1;
                                break;
                            }
                    }
                    Mode = mode;
                    Package = package;
                    Run += AlicePackageObject_EntryIOFunctions_Run;
                }
                public AlicePackageObject Package { get; set; }
                public AlicePackageObjectt_EntryIOFunctionMode Mode { get; set; }
                private void AlicePackageObject_EntryIOFunctions_Run(object sender, FunctionBaseEventArgs e)
                {
                    switch (Mode)
                    {
                        case AlicePackageObjectt_EntryIOFunctionMode.Exists:
                            {
                                e.Return = new Variable(Package.Package.ExistsEntry(e.Args[0].AsString()));
                                break;
                            }
                        case AlicePackageObjectt_EntryIOFunctionMode.ReadData:
                            {
                                e.Return = new Variable(Package.Package.GetEntryData(e.Args[0].AsString()));
                                break;
                            }
                        case AlicePackageObjectt_EntryIOFunctionMode.ReadText:
                            {
                                e.Return = new Variable(Package.Package.GetEntryText(e.Args[0].AsString()));
                                break;
                            }
                    }
                }

                public enum AlicePackageObjectt_EntryIOFunctionMode
                {
                    Exists, ReadData, ReadText
                }
            }
            private class AlicePackageObjectProperty : PropertyBase
            {
                public AlicePackageObjectProperty(AlicePackageObject host, AlicePackageObjectPropertyMode mode)
                {
                    Host = host;
                    Mode = mode;
                    Name = Mode.ToString();
                    HandleEvents = true;
                    CanSet = false;
                    Getting += AlicePackageObjectProperty_Getting;
                }

                private void AlicePackageObjectProperty_Getting(object sender, PropertyBaseEventArgs e)
                {
                    switch (Mode)
                    {
                        case AlicePackageObjectPropertyMode.Name:
                            {
                                e.Value = new Variable(Host.Package.Manifest.Name);
                                break;
                            }
                        case AlicePackageObjectPropertyMode.Version:
                            {
                                e.Value = new Variable(Host.Package.Manifest.Version);
                                break;
                            }
                        case AlicePackageObjectPropertyMode.Description:
                            {
                                e.Value = new Variable(Host.Package.Manifest.Description);
                                break;
                            }
                        case AlicePackageObjectPropertyMode.Publisher:
                            {
                                e.Value = new Variable(Host.Package.Manifest.Publisher);
                                break;
                            }
                        case AlicePackageObjectPropertyMode.Target:
                            {
                                e.Value = new Variable(Host.Package.Manifest.Target);
                                break;
                            }
                    }
                }

                public enum AlicePackageObjectPropertyMode
                {
                    Name, Version, Description, Publisher, Target
                }
                public AlicePackageObjectPropertyMode Mode { get; set; }
                public AlicePackageObject Host { get; set; }
            }
        }
    }
    internal class Interpreter_NameFunc : FunctionBase
    {
        public Interpreter_NameFunc()
        {
            Name = "Interpreter_Name";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            Run += Interpreter_NameFunc_Run;
        }

        private void Interpreter_NameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Interpreter.Instance.Name);
        }
    }
    internal class Interpreter_ScriptObject : ObjectBase
    {
        public Interpreter_ScriptObject(ParsingScript script)
        {
            Name = "Script";
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.IsMainFile, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.FileName, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.PWD, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalScript, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.FunctionName, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.StillValid, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Size, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalLineNumber, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalLine, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Labels, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Generation, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Functions, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Variables, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Consts, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Parent, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Package, this));
            AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.StackTrace, this));

            Constructor = new Interpreter_ScriptObject_Constructor();

            AddFunction(new Interpreter_ScriptObject_GetConst(this));
            AddFunction(new Interpreter_ScriptObject_GetVariable(this));
            AddFunction(new Interpreter_ScriptObject_GetFunction(this));
            AddFunction(new Interpreter_ScriptObject_ExecuteFunction(this));
            AddFunction(new Interpreter_ScriptObject_GetScriptFunction(this));
            AddFunction(new Interpreter_ScriptObject_UsingFunction(this));

            Script = script;
        }
        internal ParsingScript Script;
        public override bool Equals(ObjectBase other)
        {
            return other is Interpreter_ScriptObject iso ? iso.Script.Equals(Script) : false;
        }

        private class Interpreter_ScriptObject_Constructor : FunctionBase
        {
            public Interpreter_ScriptObject_Constructor()
            {
                MinimumArgCounts = 1;
                Run += Interpreter_ScriptObject_Constructor_Run;
            }
            private void Interpreter_ScriptObject_Constructor_Run(object sender, FunctionBaseEventArgs e)
            {
                string code = Utils.ConvertToScript(e.Args[0].AsString(), out var char2Line, out var defines, out var setting);
                var script = new ParsingScript(code, 0, char2Line);
                script.Settings = setting;
                script.Defines = defines;
                script.ParentScript = e.Script;
                e.Return = new Variable(new Interpreter_ScriptObject(script));
            }
        }
        private class Interpreter_ScriptObject_GetVariable : FunctionBase
        {
            public Interpreter_ScriptObject_GetVariable(Interpreter_ScriptObject host)
            {
                Host = host;
                Name = "GetVariable";
                MinimumArgCounts = 1;
                Run += Interpreter_ScriptObject_GetVariable_Run;
            }
            public Interpreter_ScriptObject Host { get; set; }
            private void Interpreter_ScriptObject_GetVariable_Run(object sender, FunctionBaseEventArgs e)
            {
                if (Host.Script.TryGetVariable(e.Args[0].AsString(), out ParserFunction impl) && impl is GetVarFunction vf)
                {
                    e.Return = vf.Value;
                }
            }
        }
        private class Interpreter_ScriptObject_GetConst : FunctionBase
        {
            public Interpreter_ScriptObject_GetConst(Interpreter_ScriptObject host)
            {
                Host = host;
                Name = "GetConst";
                MinimumArgCounts = 1;
                Run += Interpreter_ScriptObject_GetVariable_Run;
            }
            public Interpreter_ScriptObject Host { get; set; }
            private void Interpreter_ScriptObject_GetVariable_Run(object sender, FunctionBaseEventArgs e)
            {
                if (Host.Script.TryGetConst(e.Args[0].AsString(), out ParserFunction impl) && impl is GetVarFunction vf)
                {
                    e.Return = vf.Value;
                }
            }
        }
        private class Interpreter_ScriptObject_GetFunction : FunctionBase
        {
            public Interpreter_ScriptObject_GetFunction(Interpreter_ScriptObject host)
            {
                Host = host;
                Name = "GetFunction";
                MinimumArgCounts = 1;
                Run += Interpreter_ScriptObject_GetVariable_Run;
            }
            public Interpreter_ScriptObject Host { get; set; }
            private void Interpreter_ScriptObject_GetVariable_Run(object sender, FunctionBaseEventArgs e)
            {
                if (Host.Script.TryGetVariable(e.Args[0].AsString(), out ParserFunction impl) && impl is CustomFunction cf)
                {
                    e.Return = new Variable(cf);
                }
            }
        }
        private class Interpreter_ScriptObject_ExecuteFunction : FunctionBase
        {
            public Interpreter_ScriptObject_ExecuteFunction(Interpreter_ScriptObject host)
            {
                Host = host;
                Name = "Execute";
                Run += Interpreter_ScriptObject_ExecuteFunction_Run;
            }

            private void Interpreter_ScriptObject_ExecuteFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = Host.Script.ExecuteAll();
            }

            public Interpreter_ScriptObject Host { get; private set; }
        }
        private class Interpreter_ScriptObject_GetScriptFunction : FunctionBase
        {
            private Interpreter_ScriptObject Host { get; set; }
            public Interpreter_ScriptObject_GetScriptFunction(Interpreter_ScriptObject host)
            {
                Host = host;
                Name = "GetScript";
                Run += Interpreter_ScriptObject_GetScriptFunction_Run;
            }

            private void Interpreter_ScriptObject_GetScriptFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(new Interpreter_ScriptObject(Host.Script.GetTempScript(e.Args[0].AsString())));
            }
        }
        private class Interpreter_ScriptObject_UsingFunction : FunctionBase
        {
            private Interpreter_ScriptObject Host { get; set; }
            public Interpreter_ScriptObject_UsingFunction(Interpreter_ScriptObject host)
            {
                Host = host;
                MinimumArgCounts = 1;
                Name = "Using";
                Run += Interpreter_ScriptObject_GetScriptFunction_Run;
            }

            private void Interpreter_ScriptObject_GetScriptFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                Host.Script.Using(e.Args[0].AsString());
            }
        }
        private class Interpreter_ScriptObject_Property : PropertyBase
        {
            public Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property_Mode mode, Interpreter_ScriptObject host)
            {
                Mode = mode;
                Host = host;
                Name = Mode.ToString();
                switch (mode)
                {
                    case Interpreter_ScriptObject_Property_Mode.Parent:
                        {
                            CanSet = true;
                            break;
                        }
                    default:
                        {
                            CanSet = false;
                            break;
                        }
                }
                HandleEvents = true;
                Getting += Interpreter_ScriptObject_Property_Getting;
                Setting += Interpreter_ScriptObject_Property_Setting;
            }

            private void Interpreter_ScriptObject_Property_Setting(object sender, PropertyBaseEventArgs e)
            {
                switch (Mode)
                {
                    case Interpreter_ScriptObject_Property_Mode.Parent:
                        {
                            if (e.Value.Type == Variable.VarType.OBJECT && e.Value.Object is Interpreter_ScriptObject iso)
                            {
                                Host.Script.ParentScript = iso.Script;
                            }
                            else if (e.Value.Type == Variable.VarType.NONE)
                            {
                                Host.Script.ParentScript = null;
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            private Interpreter_ScriptObject_Property_Mode Mode;
            private Interpreter_ScriptObject Host;
            internal enum Interpreter_ScriptObject_Property_Mode
            {
                IsMainFile, FileName, PWD, OriginalScript, FunctionName, StillValid, Size, OriginalLineNumber, OriginalLine, Labels, Generation, Functions, Variables, Consts, Parent, Package, StackTrace
            }
            private void Interpreter_ScriptObject_Property_Getting(object sender, PropertyBaseEventArgs e)
            {
                switch (Mode)
                {
                    case Interpreter_ScriptObject_Property_Mode.IsMainFile:
                        {
                            bool isMain = !string.IsNullOrWhiteSpace(Host.Script.MainFilename) &&
                           Host.Script.MainFilename == Host.Script.Filename;
                            e.Value = new Variable(isMain);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.FileName:
                        {
                            e.Value = new Variable(Host.Script.Filename);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.PWD:
                        {
                            e.Value = new Variable(Host.Script.PWD);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.OriginalScript:
                        {
                            e.Value = new Variable(Host.Script.OriginalScript);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.FunctionName:
                        {
                            e.Value = new Variable(Host.Script.FunctionName);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.StillValid:
                        {
                            e.Value = new Variable(Host.Script.StillValid());
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Size:
                        {
                            e.Value = new Variable(Host.Script.Size());
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.OriginalLineNumber:
                        {
                            e.Value = new Variable(Host.Script.OriginalLineNumber);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.OriginalLine:
                        {
                            e.Value = new Variable(Host.Script.OriginalLine);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Labels:
                        {
                            Variable v = new Variable(Variable.VarType.ARRAY);
                            if (Host.Script.AllLabels == null)
                            {
                                e.Value = Variable.EmptyInstance;
                                break;
                            }
                            foreach (string s in Host.Script.AllLabels.Keys)
                            {
                                v.Tuple.Add(new Variable(s));
                            }
                            e.Value = v;
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Generation:
                        {
                            e.Value = new Variable(Host.Script.Generation);
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Functions:
                        {
                            Variable v = new Variable(Variable.VarType.ARRAY);
                            foreach (string s in Host.Script.Functions.Keys)
                            {
                                v.Tuple.Add(new Variable(s));
                            }
                            e.Value = v;
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Variables:
                        {
                            Variable v = new Variable(Variable.VarType.ARRAY);
                            foreach (string s in Host.Script.Variables.Keys)
                            {
                                v.Tuple.Add(new Variable(s));
                            }
                            e.Value = v;
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Consts:
                        {
                            Variable v = new Variable(Variable.VarType.ARRAY);
                            foreach (string s in Host.Script.Consts.Keys)
                            {
                                v.Tuple.Add(new Variable(s));
                            }
                            e.Value = v;
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Parent:
                        {
                            if (Host.Script.ParentScript != null)
                            {
                                e.Value = new Variable(new Interpreter_ScriptObject(Host.Script.ParentScript));
                            }
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.Package:
                        {
                            if (Host.Script.Package != null)
                            {
                                e.Value = new Variable(new GetPackageFunc.AlicePackageObject(Host.Script.Package));
                            }
                            break;
                        }
                    case Interpreter_ScriptObject_Property_Mode.StackTrace:
                        {
                            if (Host.Script != null)
                            {
                                e.Value = Host.Script.GetStackTrace();
                            }
                            break;
                        }
                }
            }
        }
    }
}
