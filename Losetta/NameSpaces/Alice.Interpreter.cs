using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Packaging;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

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
                space.Add<ParsingScript>();
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
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static bool Interpreter_NameExists(ParsingScript script)
        {
            string varName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            varName = Constants.ConvertName(varName);
            return ParserFunction.GetVariable(varName, script) is not null;
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
            return (script.TryGetVariable(name, out ParserFunction impl) || ParserFunction.s_functions.TryGetValue(name, out impl)) && impl is ValueFunction vf
                ? vf.Value
                : throw new ScriptException("指定された名前の変数は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, script);
        }
        public static IEnumerable<string> Interpreter_Namespaces()
        {
            return NameSpaceManager.NameSpaces.Keys;
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
                ? NameSpaceManager.NameSpaces[nameSpace].Functions.Select(item => item.Key)
                : Array.Empty<string>();
        }
        public static ParsingScript GetScript(ParsingScript script)
        {
            return script;
        }
        public static ParsingScript GetParent(ParsingScript script)
        {
            return script.ParentScript;
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
            if (t is null)
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
                AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Name));
                AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Version));
                AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Description));
                AddFunction(new AlicePackageObjectProperty(this, AlicePackageObjectProperty.AlicePackageObjectPropertyMode.Publisher));

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
            private sealed class AlicePackageObjectProperty : ValueFunction
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

                private void AlicePackageObjectProperty_Getting(object sender, ValueFunctionEventArgs e)
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

}
