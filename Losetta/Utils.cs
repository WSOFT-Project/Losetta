using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AliceScript
{
    public partial class Utils
    {
        public static void CheckArgs(int args, int expected, string msg, bool exactMatch = false)
        {
            if (args < expected || (exactMatch && args != expected))
            {
                //引数の不足
                throw new ScriptException(msg + "には引数が" + expected + "個必要ですが、" + args + "個しか指定されていません", Exceptions.INSUFFICIENT_ARGUMETS);
            }
        }

        public static void CheckNonNegativeInt(Variable variable, ParsingScript script)
        {
            CheckInteger(variable, script);
            if (variable.Value < 0)
            {
                ThrowErrorMsg("次の数の代わりに負でない整数である必要があります [" +
                              variable.Value + "]", Exceptions.EXPECTED_NON_NEGATIVE_INTEGER, script, script.Current.ToString());
            }
        }
        public static void CheckInteger(Variable variable, ParsingScript script)
        {
            CheckNumber(variable, script);
            if (variable.Value % 1 != 0.0)
            {
                ThrowErrorMsg("次の数の代わりに整数である必要があります  [" +
                              variable.Value + "]", Exceptions.EXPECTED_INTEGER, script, script.Current.ToString());
            }
        }
        public static void CheckNumber(Variable variable, ParsingScript script)
        {
            if (variable.Type != Variable.VarType.NUMBER)
            {
                ThrowErrorMsg("次の代わりに数値型である必要があります  [" +
                              variable.AsString() + "]", Exceptions.WRONG_TYPE_VARIABLE, script, script.Current.ToString());
            }
        }
        public static void CheckNotNull(string name, ParserFunction func, ParsingScript script)
        {
            if (func == null)
            {
                string realName = Constants.GetRealName(name);
                ThrowErrorMsg("次の変数または関数は存在しません [" + realName + "]", Exceptions.PROPERTY_OR_METHOD_NOT_FOUND, script, name);
            }
        }
        public static bool CheckNotNull(object obj, string name, ParsingScript script)
        {
            if (obj == null)
            {
                string realName = Constants.GetRealName(name);
                ThrowErrorMsg("次のオブジェクトは存在しません [" + realName + "]", Exceptions.OBJECT_DOESNT_EXIST, script, name);
                return false;
            }
            return true;
        }

        public static void CheckNotEnd(ParsingScript script)
        {
            if (!script.StillValid())
            {
                ThrowErrorMsg("関数の定義が不完全です", Exceptions.INCOMPLETE_FUNCTION_DEFINITION, script, script.Prev.ToString());
            }
        }
        public static void CheckForValidName(string name, ParsingScript script)
        {
            if (string.IsNullOrWhiteSpace(name) || (!Char.IsLetter(name[0]) && name[0] != '_'))
            {
                ThrowErrorMsg("変数名として次の名前は使用できません: [" + name + "]", Exceptions.ILLEGAL_VARIABLE_NAME,
                              script, name);
            }

            string illegals = "\"'?!";
            int first = name.IndexOfAny(illegals.ToCharArray());
            if (first >= 0)
            {
                var ind = name.IndexOf('[');
                if (ind < 0 || ind > first)
                {
                    for (int i = 0; i < illegals.Length; i++)
                    {
                        char ch = illegals[i];
                        if (name.Contains(ch))
                        {
                            ThrowErrorMsg("[" + name + "]のうち、変数名として [" + ch + "]は使用できません", Exceptions.CONTAINS_ILLEGAL_CHARACTER,
                                          script, name);
                        }
                    }
                }
            }
        }

        public static void ThrowErrorMsg(string msg, Exceptions errorcode, ParsingScript script, string token)
        {
            /*
             * TODO:ThrowErrorMSGの引継ぎ等
            string code     = script == null || string.IsNullOrWhiteSpace(script.OriginalScript) ? "" : script.OriginalScript;
            int lineNumber  = script == null ? 0 : script.OriginalLineNumber;
            string filename = script == null || string.IsNullOrWhiteSpace(script.Filename) ? "" : script.Filename;
            int minLines    = script == null || script.OriginalLine.ToLower().Contains(token.ToLower()) ? 1 : 2;

            ThrowErrorMsg(msg, code, lineNumber, filename, minLines);
            */
            throw new ScriptException(msg, errorcode, script);
        }

        private static void ThrowErrorMsg(string msg, string script, Exceptions ecode, int lineNumber, string filename = "", int minLines = 1)
        {
            string[] lines = script.Split('\n');
            lineNumber = lines.Length <= lineNumber ? -1 : lineNumber;
            System.Diagnostics.Debug.WriteLine(msg);
            if (lineNumber < 0)
            {
                throw new ParsingException(msg);
            }

            var currentLineNumber = lineNumber;
            var line = lines[lineNumber].Trim();
            var collectMore = line.Length < 3 || minLines > 1;
            var lineContents = line;

            while (collectMore && currentLineNumber > 0)
            {
                line = lines[--currentLineNumber].Trim();
                collectMore = line.Length < 2 || (minLines > lineNumber - currentLineNumber + 1);
                lineContents = line + "  " + lineContents;
            }

            if (lines.Length > 1)
            {
                string lineStr = currentLineNumber == lineNumber ? "行: " + (lineNumber + 1) :
                                 "行" + (currentLineNumber + 1) + "-" + (lineNumber + 1);
                msg += " " + lineStr + ": " + lineContents;
            }

            StringBuilder stack = new StringBuilder();
            stack.AppendLine("" + currentLineNumber);
            stack.AppendLine(filename);
            stack.AppendLine(line);
            throw new ScriptException(msg + stack.ToString(), ecode);
        }

        private static void ThrowErrorMsg(string msg, string code, Exceptions ecode, int level, int lineStart, int lineEnd, string filename)
        {
            var lineNumber = level > 0 ? lineStart : lineEnd;
            ThrowErrorMsg(msg, code, ecode, lineNumber, filename);
        }

        public static bool CheckLegalName(string name, ParsingScript script = null, bool throwError = true)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            if (Constants.CheckReserved(name))
            {
                if (!throwError)
                {
                    return false;
                }
                Utils.ThrowErrorMsg(name + "は予約語のため使用できません", Exceptions.ITS_RESERVED_NAME, script, name);
            }
            if (Char.IsDigit(name[0]) || name[0] == '-')
            {
                if (!throwError)
                {
                    return false;
                }
                Utils.ThrowErrorMsg(name + "として定義されていますが、[" + name[0] + "]を変数名の先端に使用することはできません", Exceptions.ITHAS_ILLEGAL_FIRST_CHARACTER, null, name);
            }
            return true;
        }

        public static ParsingScript GetTempScript(string str, ParserFunction.StackLevel stackLevel, string name = "",
            ParsingScript script = null, ParsingScript parentScript = null,
            int parentOffset = 0, AliceScriptClass.ClassInstance instance = null)
        {
            ParsingScript tempScript = new ParsingScript(str);
            tempScript.ScriptOffset = parentOffset;
            if (parentScript != null)
            {
                tempScript.Char2Line = parentScript.Char2Line;
                tempScript.Filename = parentScript.Filename;
                tempScript.OriginalScript = parentScript.OriginalScript;
            }
            tempScript.ParentScript = script;
            tempScript.InTryBlock = script == null ? false : script.InTryBlock;
            tempScript.ClassInstance = instance;
            tempScript.StackLevel = stackLevel;
            if (script != null)
            {
                tempScript.Package = script.Package;
                tempScript.Tag = script.Tag;
            }

            return tempScript;
        }

        public static bool ExtractParameterNames(List<Variable> args, string functionName, ParsingScript script)
        {
            FunctionBase custFunc = ParserFunction.GetFunction(functionName, script) as FunctionBase;
            if (custFunc == null)
            {
                return false;
            }

            var realArgs = custFunc.RealArgs;
            for (int i = 0; i < args.Count && i < realArgs.Length; i++)
            {
                string name = args[i].CurrentAssign;
                args[i].ParamName = string.IsNullOrWhiteSpace(name) ? realArgs[i] : name;
            }
            return true;
        }



        /// <summary>
        /// 現在のパッケージまたはローカルからファイルを取得します
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="fromPackage">パッケージからのみファイルを取得する場合はTrue、それ以外の場合はFalse。</param>
        /// <param name="script">現在のパッケージを表すスクリプト</param>
        /// <returns></returns>
        public static byte[] GetFileFromPackageOrLocal(string filename, bool fromPackage = false, ParsingScript script = null)
        {
            if (script != null && script.Package != null && script.Package.ExistsEntry(filename))
            {
                return script.Package.GetEntryData(filename);
            }
            if (fromPackage || !File.Exists(filename))
            {
                throw new ScriptException("ファイル名:[" + filename + "]は存在しません", Exceptions.FILE_NOT_FOUND);
            }
            return File.ReadAllBytes(filename);
        }
        public static string GetFileLines(string filename)
        {
            string lines = SafeReader.ReadAllText(filename, out var v);
            if (lines == null)
            {
                lines = string.Empty;
            }
            return lines;
        }
        public static string GetFileLines(byte[] data)
        {
            string lines = SafeReader.ReadAllText(data, out _);
            if (lines == null)
            {
                lines = string.Empty;
            }
            return lines;
        }

        public static GetVarFunction ExtractArrayElement(string token)
        {
            if (!token.Contains(Constants.START_ARRAY))
            {
                return null;
            }

            ParsingScript tempScript = new ParsingScript(token);
            Variable result = tempScript.Execute();
            return new GetVarFunction(result);
        }



        public static void PrintList(List<Variable> list, int from)
        {
            Console.Write("Merging list:");
            for (int i = from; i < list.Count; i++)
            {
                Console.Write(" ({0}, '{1}')", list[i].Value, list[i].Action);
            }
            Console.WriteLine();
        }

        public static int GetSafeInt(List<Variable> args, int index, int defaultValue = 0)
        {
            if (args.Count <= index)
            {
                return defaultValue;
            }
            Variable numberVar = args[index];
            if (numberVar.Type != Variable.VarType.NUMBER)
            {
                if (string.IsNullOrWhiteSpace(numberVar.String))
                {
                    return defaultValue;
                }
                int num;
                if (!Int32.TryParse(numberVar.String, NumberStyles.Number,
                                     CultureInfo.InvariantCulture, out num))
                {
                    throw new ArgumentException("Expected an integer instead of [" + numberVar.AsString() + "]");
                }
                return num;
            }
            return numberVar.AsInt();
        }

        public static string GetSafeString(List<Variable> args, int index, string defaultValue = "")
        {
            if (args.Count <= index)
            {
                return defaultValue;
            }
            return args[index].AsString();
        }
        public static bool GetSafeBool(List<Variable> args, int index, bool defaultValue = false)
        {
            if (args.Count <= index)
            {
                return defaultValue;
            }
            return args[index].AsBool();
        }
        public static Variable GetSafeVariable(List<Variable> args, int index, Variable defaultValue = null)
        {
            if (args.Count <= index)
            {
                return defaultValue;
            }
            return args[index];
        }

        public static Variable GetVariable(string varName, ParsingScript script = null, bool testNull = true)
        {
            varName = varName.ToLower();
            if (script == null)
            {
                script = new ParsingScript("");
            }

            ParserFunction func = ParserFunction.GetVariable(varName, script);
            if (!testNull && func == null)
            {
                return null;
            }
            Utils.CheckNotNull(varName, func, script);
            Variable varValue = func.GetValue(script);
            Utils.CheckNotNull(varValue, varName, script);
            return varValue;
        }


        public static double ConvertToDouble(object obj, ParsingScript script = null)
        {
            string str = obj.ToString().ToLower();
            double num = 0;
            if (script.Tag is string s && s == "DELEGATE") { return 0; }
            if (!CanConvertToDouble(str, out num) &&
                script != null)
            {
                ProcessErrorMsg(str, script);
            }
            return num;
        }

        public static bool CanConvertToDouble(string str, out double num)
        {
            num = 0;
            //文字列を小文字に置き換え
            str = str.ToLower();
            //0xから始まる実数の16進表現を確認します
            System.Text.RegularExpressions.MatchCollection mc =
    System.Text.RegularExpressions.Regex.Matches(
    str, @"0x[0-9a-f]+");
            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                try
                {
                    //16進表現では浮動小数点型の表現ができないためdoubleと最も近い精度である整数値型long(Int64)を使用します
                    num = long.Parse(m.Value.Substring(2), NumberStyles.HexNumber);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            //0bから始まる実数の2進表現を確認します
            mc = System.Text.RegularExpressions.Regex.Matches(
    str, @"0b[0-9a-f]+");
            foreach (System.Text.RegularExpressions.Match m in mc)
            {
                try
                {
                    //2進表現では浮動小数点型の表現ができないためdoubleと最も近い精度である整数値型long(Int64)を使用します
                    num = Convert.ToInt64(m.Value.Substring(2), 2);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return Double.TryParse(str, NumberStyles.Number |
                                        NumberStyles.AllowExponent |
                                        NumberStyles.Float,
                                        CultureInfo.InvariantCulture, out num);
        }

        public static void ProcessErrorMsg(string str, ParsingScript script)
        {
            if (!string.IsNullOrEmpty(str))
            {
                char ch = script.TryPrev();
                string entity = ch == '(' ? "関数" :
                                ch == '[' ? "配列" :
                                ch == '{' ? "演算子" :
                                            "変数";
                Exceptions ex = ch == '(' ? Exceptions.COULDNT_FIND_FUNCTION :
                                ch == '[' ? Exceptions.COULDNT_FIND_ARRAY :
                                ch == '{' ? Exceptions.COULDNT_FIND_OPERATOR :
                                            Exceptions.COULDNT_FIND_VARIABLE;
                string token = Constants.GetRealName(str);

                string msg = entity + ":[" + token + "]は定義されていないか、存在しません";

                ThrowErrorMsg(msg, ex, script, str);
            }

        }

        public static int GetNumberOfDigits(string data, int itemNumber = -1)
        {
            if (itemNumber >= 0)
            {
                string[] vals = data.Split(new char[] { ',', ':' });
                if (vals.Length <= itemNumber)
                {
                    return 0;
                }
                int min = 0;
                for (int i = 0; i < vals.Length; i++)
                {
                    min = Math.Max(min, GetNumberOfDigits(vals[i]));
                }
                return min;
            }

            int index = data.IndexOf(".");
            if (index < 0 || index >= data.Length - 1)
            {
                return 0;
            }
            return data.Length - index - 1;
        }
        public static string GetFileContents(byte[] data)
        {
            return Utils.GetFileLines(data).Replace(Environment.NewLine, Constants.END_LINE.ToString());
        }


        public static string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
            {
                return path;
            }
            path = Path.GetFullPath(path);
            return path;
        }

        public static string GetDirectoryName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return GetCurrentDirectory();
            }
            return Path.GetDirectoryName(path);
        }

        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
