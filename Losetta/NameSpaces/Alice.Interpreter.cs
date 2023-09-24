using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Interop;
using AliceScript.Objects;
using AliceScript.Packaging;
using AliceScript.Parsing;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace AliceScript.NameSpaces
{
    internal static class Alice_Interpreter_Initer
    {
        public static void Init()
        {
            try
            {
                Alice.RegisterFunctions<InterpreterFunctions>();
                NameSpace space = new NameSpace("Alice.Interpreter");
                space.Add(new Interpreter_ScriptObject(null));
                space.Add(new TypeObject());

                NameSpaceManager.Add(space);
            }
            catch { }
        }
    }
    [AliceNameSpace(Name = "Alice.Interpreter")]
    internal sealed class InterpreterFunctions
    {
        public static void Interpreter_Reset_Variables()
        {
            ParserFunction.CleanUpVariables();
        }
        public static void Interpreter_Append_Data(string text, bool newLine = false)
        {
            Interpreter.Instance.AppendDebug(text, newLine);
        }
        public static void Interpreter_Append_Output(string text, bool newLine = false)
        {
            Interpreter.Instance.AppendOutput(text, newLine);
        }
        public static Interpreter_ScriptObject Interpreter_GetParent(ParsingScript script)
        {
            return new Interpreter_ScriptObject(script.ParentScript);
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static bool Interpreter_NameExists(ParsingScript script)
        {
            string varName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            varName = Constants.ConvertName(varName);
            return ParserFunction.GetVariable(varName, script) != null;
        }
        public static string Interpreter_Name()
        {
            return Interpreter.Instance.Name;
        }
        public static Variable Interpreter_ProcessFile(string fileName, bool mainFile = false)
        {
            return Interpreter.Instance.ProcessFile(fileName, mainFile);
        }
        public static Variable Interpreter_Process(string script, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.Process(script, filename, mainFile);
        }
        public static Variable Interpreter_GetVariable(ParsingScript script, string name)
        {
            return (script.TryGetVariable(name, out ParserFunction impl) || ParserFunction.s_functions.TryGetValue(name, out impl)) && impl is GetVarFunction vf
                ? vf.Value
                : throw new ScriptException("指定された名前の変数は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, script);
        }
        public static IEnumerable<string> Interpreter_Namespaces()
        {
            return NameSpaceManager.NameSpaces.Keys;
        }
        public static IEnumerable<string> Interpreter_GlobalVariables()
        {
            return ParserFunction.s_variables.Keys;
        }
        public static IEnumerable<string> Interpreter_Consts()
        {
            return Constants.CONSTS.Keys;
        }
        public static IEnumerable<string> Interpreter_Functions()
        {
            return FunctionBaseManager.Functions;
        }
        public static IEnumerable<string> Interpreter_Functions(string nameSpace)
        {
            return NameSpaceManager.Contains(nameSpace)
                ? NameSpaceManager.NameSpaces[nameSpace].Functions.Select(item => item.Name)
                : Array.Empty<string>();
        }
        public static Interpreter_ScriptObject Interpreter_GetScript(ParsingScript script)
        {
            return new Interpreter_ScriptObject(script);
        }

        #region ガページコレクション
        public static void GC_Collect()
        {
            GC.Collect();
        }
        public static void GC_Collect(int generation)
        {
            GC.Collect(generation);
        }
        public static int GC_CollectionCount(int generation)
        {
            return GC.CollectionCount(generation);
        }
        public static long GC_GetTotalMemory(bool forceFullCollection = false)
        {
            return GC.GetTotalMemory(forceFullCollection);
        }
        #endregion

        #region バインド
        public static void Bind_Register(string name)
        {
            Type t = Type.GetType(name);
            if (t == null)
            {
                throw new ScriptException($"{name}という名前の型を検索できませんでした。アセンブリ名の指定を忘れていませんか？", Exceptions.OBJECT_DOESNT_EXIST);
            }
            NameSpaceManager.Add(t);
        }
        #endregion

        public static AlicePackageObject Interpreter_GetPackage(ParsingScript script)
        {
            return new AlicePackageObject(script.Package);
        }
        internal sealed class AlicePackageObject : ObjectBase
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
            private sealed class AlicePackageObject_EntryIOFunctions : FunctionBase
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
            private sealed class AlicePackageObjectProperty : PropertyBase
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



    internal sealed class Interpreter_ScriptObject : ObjectBase
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
            return other is Interpreter_ScriptObject iso && iso.Script.Equals(Script);
        }

        private sealed class Interpreter_ScriptObject_Constructor : FunctionBase
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
        private sealed class Interpreter_ScriptObject_GetVariable : FunctionBase
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
        private sealed class Interpreter_ScriptObject_GetConst : FunctionBase
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
        private sealed class Interpreter_ScriptObject_GetFunction : FunctionBase
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
        private sealed class Interpreter_ScriptObject_ExecuteFunction : FunctionBase
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
        private sealed class Interpreter_ScriptObject_GetScriptFunction : FunctionBase
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
        private sealed class Interpreter_ScriptObject_UsingFunction : FunctionBase
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
                                e.Value = new Variable(new InterpreterFunctions.AlicePackageObject(Host.Script.Package));
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
