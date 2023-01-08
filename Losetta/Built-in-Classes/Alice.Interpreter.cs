using System;
using System.Collections.Generic;

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
                space.Add(new Interpreter_GetScriptFunc());
                space.Add(new gc_collectFunc());
                space.Add(new gc_gettotalmemoryFunc());
                space.Add(new gc_collectafterexecuteFunc());
                space.Add(new TypeObject());

                NameSpaceManerger.Add(space);
            }
            catch { }
        }
    }

    internal class Interpreter_GetParentFunc : FunctionBase
    {
        public Interpreter_GetParentFunc()
        {
            this.Name = "Interpreter_GetParent";
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_GetParentFunc_Run;
        }

        private void Interpreter_GetParentFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type==Variable.VarType.OBJECT && e.Args[0].Object is Interpreter_ScriptObject o)
            {
                e.Return = new Variable(new Interpreter_ScriptObject(o.Script.ParentScript));
            }
            else if (e.Args[0].Parent!=null)
            {
                e.Return = new Variable(new Interpreter_ScriptObject(e.Args[0].Parent));
            }
        }
    }
    internal class Interpreter_NameExistsFunc : FunctionBase
    {
        public Interpreter_NameExistsFunc()
        {
            this.Name = "Interpreter_NameExists";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += Interpreter_NameExistsFunc_Run;
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
            this.Name = "gc_collect";
            this.MinimumArgCounts = 0;
            this.Run += Gc_collectFunc_Run;
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
            this.Name = "gc_gettotalmemory";
            this.MinimumArgCounts = 1;
            this.Run += Gc_gettotalmemoryFunc_Run;
        }

        private void Gc_gettotalmemoryFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            GC.GetTotalMemory(e.Args[0].AsBool());
        }
    }

    internal class gc_collectafterexecuteFunc : FunctionBase
    {
        public gc_collectafterexecuteFunc()
        {
            this.Name = "gc_collectafterexecute";
            this.MinimumArgCounts = 0;
            this.Run += Gc_collectafterexecuteFunc_Run;
        }

        private void Gc_collectafterexecuteFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                AliceScript.Interop.GCManerger.CollectAfterExecute = e.Args[0].AsBool();
            }
            e.Return = new Variable(Interop.GCManerger.CollectAfterExecute);
        }
    }

    internal class Interpreter_Reset_VariablesFunc : FunctionBase
    {
        public Interpreter_Reset_VariablesFunc()
        {
            this.Name = "Interpreter_Reset_Variables";
            this.Run += Interpreter_Reset_VariablesFunc_Run;
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
            if (isdata)
            {
                this.Name = "Interpreter_Append_Data";
            }
            else
            {
                this.Name = "Interpreter_Append_Output";
            }
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_Append_OutputOrDataFunc_Run;
        }

        private void Interpreter_Append_OutputOrDataFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_isData)
            {
                e.Return = new Variable(Interpreter.Instance.AppendData(e.Args[0].AsString(), (Utils.GetSafeBool(e.Args, 1))));
            }
            else
            {
                Interpreter.Instance.AppendOutput(e.Args[0].AsString(), (Utils.GetSafeBool(e.Args, 1)));
            }
        }

        private bool m_isData = false;
    }

    internal class Interpreter_ProcessOrFileFunc : FunctionBase
    {
        public Interpreter_ProcessOrFileFunc(bool isfile = false)
        {
            m_isFile = isfile;
            if (m_isFile) { this.Name = "Interpreter_ProcessFile"; } else { this.Name = "Interpreter_Process"; }
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_ProcessOrFileFunc_Run;
        }

        private void Interpreter_ProcessOrFileFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_isFile)
            {
                e.Return = Interpreter.Instance.ProcessFile(e.Args[0].AsString(), (Utils.GetSafeBool(e.Args, 1)));
            }
            else
            {
                e.Return = Interpreter.Instance.Process(e.Args[0].AsString(), Utils.GetSafeString(e.Args, 1), (Utils.GetSafeBool(e.Args, 2)));
            }
        }

        private bool m_isFile = false;
    }

    internal class Interpreter_FunctionsFunc : FunctionBase
    {
        public Interpreter_FunctionsFunc()
        {
            this.Name = "Interpreter_Functions";
            this.Run += FunctionsFunc_Run;
        }

        private void FunctionsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                Variable v = new Variable(Variable.VarType.ARRAY);
                foreach (string s in FunctionBaseManerger.Functions)
                {
                    v.Tuple.Add(new Variable(s));
                }
                e.Return = v;
            }
            else
            {
                string str = Utils.GetSafeString(e.Args, 0);
                if (NameSpaceManerger.Contains(str))
                {
                    Variable v = new Variable(Variable.VarType.ARRAY);
                    foreach (FunctionBase fb in NameSpaceManerger.NameSpaces[str].Functions)
                    {
                        v.Tuple.Add(new Variable(fb.Name));
                    }
                    e.Return = v;
                }
                else
                {
                    throw new System.Exception("指定された名前空間が見つかりませんでした");
                }
            }
        }
    }

    internal class Interpreter_NamespacesFunc : FunctionBase
    {
        public Interpreter_NamespacesFunc()
        {
            this.Name = "Interpreter_Namespaces";
            this.Run += NamespacesFunc_Run;
        }

        private void NamespacesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable v = new Variable(Variable.VarType.ARRAY);
            foreach (string s in NameSpaceManerger.NameSpaces.Keys)
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
            this.Name = "Interpreter_GlobalVariables";
            this.Run += Interpreter_VariablesFunc_Run;
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
            this.Name = "Interpreter_GetVariable";
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_GetVariable_Run;
        }

        private void Interpreter_GetVariable_Run(object sender, FunctionBaseEventArgs e)
        {
            ParserFunction impl;
            string name = e.Args[0].AsString();
            if ((e.Script.TryGetVariable(name, out impl) || ParserFunction.s_functions.TryGetValue(name, out impl)) && impl is GetVarFunction vf)
            {
                e.Return = vf.Value;
            }
            else
            {
                throw new ScriptException("指定された名前の変数は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, e.Script);
            }
        }
    }

   

    internal class Interpreter_GetScriptFunc : FunctionBase
    {
        public Interpreter_GetScriptFunc()
        {
            this.Name = "Interpreter_GetScript";
            this.Run += Interpreter_GetScriptFunc_Run;
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
            this.Name = "Interpreter_Consts";
            this.MinimumArgCounts = 0;
            this.Attribute = FunctionAttribute.GENERAL;
            this.Run += Interpreter_ConstsFunc_Run;
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
            this.Name = "Interpreter_GetPackage";
            this.Run += GetPackageFunc_Run;
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
                this.Name = "AlicePackage";
                Package = package;
                this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
                this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
                this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
                this.AddProperty(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));

                this.AddFunction(new AlicePackageObject_EntryIOFunctions(this, AlicePackageObject_EntryIOFunctions.AlicePackageObjectt_EntryIOFunctionMode.ReadData));
                this.AddFunction(new AlicePackageObject_EntryIOFunctions(this, AlicePackageObject_EntryIOFunctions.AlicePackageObjectt_EntryIOFunctionMode.ReadText));
                this.AddFunction(new AlicePackageObject_EntryIOFunctions(this, AlicePackageObject_EntryIOFunctions.AlicePackageObjectt_EntryIOFunctionMode.Exists));
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
                                this.Name = "entry_exists";
                                this.MinimumArgCounts = 1;
                                break;
                            }
                        case AlicePackageObjectt_EntryIOFunctionMode.ReadData:
                            {
                                this.Name = "entry_read_data";
                                this.MinimumArgCounts = 1;
                                break;
                            }
                        case AlicePackageObjectt_EntryIOFunctionMode.ReadText:
                            {
                                this.Name = "entry_read_text";
                                this.MinimumArgCounts = 1;
                                break;
                            }
                    }
                    Mode = mode;
                    Package = package;
                    this.Run += AlicePackageObject_EntryIOFunctions_Run;
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
                    this.Name = Mode.ToString();
                    this.HandleEvents = true;
                    this.CanSet = false;
                    this.Getting += AlicePackageObjectProperty_Getting;
                }

                private void AlicePackageObjectProperty_Getting(object sender, PropertyGettingEventArgs e)
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
            this.Name = "Interpreter_Name";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.Run += Interpreter_NameFunc_Run;
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
            this.Name = "Script";
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.IsMainFile, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.FileName, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.PWD, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalScript, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.FunctionName, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.InTryBlock, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.StillValid, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Size, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalLineNumber, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.OriginalLine, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Labels, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Generation, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Functions, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Variables, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Consts, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Parent, this));
            this.AddProperty(new Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property.Interpreter_ScriptObject_Property_Mode.Package, this));

            this.AddFunction(new Interpreter_ScriptObject_GetConst(this));
            this.AddFunction(new Interpreter_ScriptObject_GetVariable(this));
            this.AddFunction(new Interpreter_ScriptObject_GetFunction(this));
            this.AddFunction(new Interpreter_ScriptObject_ExecuteFunction(this));
            this.AddFunction(new Interpreter_ScriptObject_GetScriptFunction(this));

            Script = script;
        }
        internal ParsingScript Script;


        private class Interpreter_ScriptObject_GetVariable : FunctionBase
        {
            public Interpreter_ScriptObject_GetVariable(Interpreter_ScriptObject host)
            {
                this.Host = host;
                this.Name = "GetVariable";
                this.MinimumArgCounts = 1;
                this.Run += Interpreter_ScriptObject_GetVariable_Run;
            }
            public Interpreter_ScriptObject Host { get; set; }
            private void Interpreter_ScriptObject_GetVariable_Run(object sender, FunctionBaseEventArgs e)
            {
                ParserFunction impl;
                if (Host.Script.TryGetVariable(e.Args[0].AsString(), out impl) && impl is GetVarFunction vf)
                {
                    e.Return = vf.Value;
                }
            }
        }
        private class Interpreter_ScriptObject_GetConst : FunctionBase
        {
            public Interpreter_ScriptObject_GetConst(Interpreter_ScriptObject host)
            {
                this.Host = host;
                this.Name = "GetConst";
                this.MinimumArgCounts = 1;
                this.Run += Interpreter_ScriptObject_GetVariable_Run;
            }
            public Interpreter_ScriptObject Host { get; set; }
            private void Interpreter_ScriptObject_GetVariable_Run(object sender, FunctionBaseEventArgs e)
            {
                ParserFunction impl;
                if (Host.Script.TryGetConst(e.Args[0].AsString(), out impl) && impl is GetVarFunction vf)
                {
                    e.Return = vf.Value;
                }
            }
        }
        private class Interpreter_ScriptObject_GetFunction : FunctionBase
        {
            public Interpreter_ScriptObject_GetFunction(Interpreter_ScriptObject host)
            {
                this.Host = host;
                this.Name = "GetFunction";
                this.MinimumArgCounts = 1;
                this.Run += Interpreter_ScriptObject_GetVariable_Run;
            }
            public Interpreter_ScriptObject Host { get; set; }
            private void Interpreter_ScriptObject_GetVariable_Run(object sender, FunctionBaseEventArgs e)
            {
                ParserFunction impl;
                if (Host.Script.TryGetVariable(e.Args[0].AsString(), out impl) && impl is CustomFunction cf)
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
                this.Name = "Execute";
                this.Run += Interpreter_ScriptObject_ExecuteFunction_Run;
            }

            private void Interpreter_ScriptObject_ExecuteFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = Host.Script.Process();
            }

            public Interpreter_ScriptObject Host { get; private set; }
        }
        private class Interpreter_ScriptObject_GetScriptFunction : FunctionBase
        {
            private Interpreter_ScriptObject Host { get; set; }
            public Interpreter_ScriptObject_GetScriptFunction(Interpreter_ScriptObject host)
            {
                Host = host;
                this.Name = "GetScript";
                this.Run += Interpreter_ScriptObject_GetScriptFunction_Run;
            }

            private void Interpreter_ScriptObject_GetScriptFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(new Interpreter_ScriptObject(Host.Script.GetTempScript(e.Args[0].AsString())));
            }
        }
        private class Interpreter_ScriptObject_Property : PropertyBase
        {
            public Interpreter_ScriptObject_Property(Interpreter_ScriptObject_Property_Mode mode, Interpreter_ScriptObject host)
            {
                Mode = mode;
                Host = host;
                this.Name = Mode.ToString();
                this.CanSet = false;
                this.HandleEvents = true;
                this.Getting += Interpreter_ScriptObject_Property_Getting;
            }
            private Interpreter_ScriptObject_Property_Mode Mode;
            private Interpreter_ScriptObject Host;
            internal enum Interpreter_ScriptObject_Property_Mode
            {
                IsMainFile, FileName, PWD, OriginalScript, FunctionName, InTryBlock, StillValid, Size, OriginalLineNumber, OriginalLine, Labels, Generation, Functions, Variables, Consts, Parent, Package
            }
            private void Interpreter_ScriptObject_Property_Getting(object sender, PropertyGettingEventArgs e)
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
                    case Interpreter_ScriptObject_Property_Mode.InTryBlock:
                        {
                            e.Value = new Variable(Host.Script.InTryBlock);
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
                }
            }
        }
    }

    public class TypeObject : ObjectBase
    {
        public TypeObject()
        {
            Init();
        }
        public TypeObject(Variable.VarType type)
        {
            Init();
            Type = type;
        }
        public TypeObject(AliceScriptClass type)
        {
            Init();
            this.ClassType = type;
            foreach (var kvs in type.StaticFunctions)
            {
                this.Functions.Add(kvs.Key, kvs.Value);
            }
        }
        private void Init()
        {
            this.Name = "Type";
            this.Functions.Add("Activate", new ActivateFunction(this));
            this.Functions.Add("ToString", new ToStringFunction(this));
            this.Functions.Add("ToNativeProperty",new ToNativeProperty(this));
            this.Properties.Add("IsObject",new IsObjectProperty(this));
            this.Properties.Add("Namespace",new NamespaceProperty(this));
            this.Properties.Add("Base",new BaseProperty(this));
        }
        public Variable.VarType Type { get; set; }
        public AliceScriptClass ClassType { get; set; }
        internal class NamespaceProperty : PropertyBase
        {
            public NamespaceProperty(TypeObject type)
            {
                this.Name = "Namespace";
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += delegate (object sender, PropertyGettingEventArgs e)
                {
                    if (type.ClassType != null)
                    {
                        e.Value = new Variable(type.ClassType.Namespace);
                    }
                    else
                    {
                        e.Value = Variable.EmptyInstance;
                    }
                };
            }
        }
        internal class BaseProperty : PropertyBase
        {
            public BaseProperty(TypeObject type)
            {
                this.Name = "Base";
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += delegate (object sender, PropertyGettingEventArgs e)
                {
                    if (type.ClassType != null)
                    {
                        e.Value = new Variable(type.ClassType.BaseClasses);
                    }
                    else
                    {
                        e.Value = Variable.EmptyInstance;
                    }
                };
            }
        }
        internal class IsObjectProperty : PropertyBase
        {
            public IsObjectProperty(TypeObject type)
            {
                this.Name = "IsObject";
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += delegate (object sender, PropertyGettingEventArgs e)
                {
                    e.Value = new Variable(type.ClassType!=null);
                };
            }
        }
        internal class ToNativeProperty : FunctionBase
        {
            public ToNativeProperty(TypeObject type)
            {
                this.Name = "ToNativeProperty";
                this.Run += delegate (object sender, FunctionBaseEventArgs e)
                {
                    if (type.ClassType != null)
                    {
                        e.Return = new Variable(Variable.VarType.OBJECT);
                    }
                    else
                    {
                        e.Return = new Variable(new TypeObject(type.Type));
                    }
                };
            }
        }
        public bool Equals(TypeObject other)
        {
            if(this.ClassType!=null && other.ClassType != null)
            {
                return ClassType.ToString() == other.ClassType.ToString();
            }
            else if(this.ClassType != null || other.ClassType != null)
            {
                return false;
            }
            else
            {
                return this.Type==other.Type;
            }
        }
        internal class ActivateFunction : FunctionBase
        {
            public ActivateFunction(TypeObject type)
            {
                this.Name = "Activate";
                this.Run += Type_ActivateFunc_Run;
                this.Type = type;
            }
            public TypeObject Type { get; set; }
            private void Type_ActivateFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                if (Type.ClassType != null)
                {
                    //TODO:非ObjectBaseのクラスのアクティベート
                    ObjectBase csClass = Type.ClassType as ObjectBase;
                    if (csClass != null)
                    {
                        Variable obj = csClass.GetImplementation(e.Args, e.Script);
                        e.Return = obj;
                        return;
                    }
                }
                else
                {
                    e.Return = new Variable(Type.Type);
                }
            }
        }
        internal class ToStringFunction : FunctionBase
        {
            public ToStringFunction(TypeObject type)
            {
                this.Name = "ToString";
                this.Run += ToStringFunction_Run;
                this.Type = type;
            }
            public TypeObject Type { get; set; }
            private void ToStringFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                if(Type.ClassType !=null && Type.ClassType is TypeObject to)
                {
                    e.Return = new Variable("Alice.Interpreter.Type");
                    return;
                }
                if (Type.ClassType != null)
                {
                    e.Return = new Variable(Type.ClassType.ToString());
                }
                else
                {
                    e.Return = new  Variable(Constants.TypeToString(Type.Type));
                }
            }
        }
    }
}
