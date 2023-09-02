using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// 指定した変数が数値を表し、かつ特定範囲内にあるかどうかを確認し、条件を満たさない場合に例外を発生します。
        /// </summary>
        /// <param name="variable">確認する変数</param>
        /// <param name="min">特定範囲の最小値</param>
        /// <param name="max">特定範囲の最大値</param>
        /// <param name="needInteger">整数かつInt32の範囲内である必要がある場合はtrue。この値は省略できます。</param>
        /// <param name="script">確認元のスクリプト</param>
        public static void CheckNumInRange(Variable variable, bool needInteger = false, double? min = null, double? max = null, ParsingScript script = null)
        {
            CheckNumber(variable, script);
            double trueMax = max == null ? (needInteger ? int.MaxValue : double.MaxValue) : max.Value;
            double trueMin = max == null ? (needInteger ? int.MinValue : double.MinValue) : min.Value;
            bool type = !needInteger || variable.Value % 1 != 0.0;
            bool less = variable.Value < trueMin;
            bool over = variable.Value > trueMax;
            if (type || less || over)
            {
                throw new ScriptException($"数値は{(min != null ? $" {min}以上かつ" : string.Empty)}{(max != null ? $" {max}以下の" : string.Empty)}{(needInteger ? "整数" : "実数")}である必要があります。", Exceptions.NUMBER_OUT_OF_RANGE, script);
            }
        }
        public static void CheckNumber(Variable variable, ParsingScript script, bool acceptNaN = false)
        {
            if (variable.Type != Variable.VarType.NUMBER && (acceptNaN || variable.Value != double.NaN))
            {
                ThrowErrorMsg("型が一致しないか、変換できません。", Exceptions.WRONG_TYPE_VARIABLE, script);
            }
        }
        public static void CheckNotNull(string name, ParserFunction func, ParsingScript script)
        {
            if (func == null)
            {
                string realName = Constants.GetRealName(name);
                ThrowErrorMsg("次の変数または関数は存在しません [" + realName + "]", Exceptions.PROPERTY_OR_METHOD_NOT_FOUND, script);
            }
        }
        public static bool CheckNotNull(object obj, string name, ParsingScript script)
        {
            if (obj == null)
            {
                string realName = Constants.GetRealName(name);
                ThrowErrorMsg("次のオブジェクトは存在しません [" + realName + "]", Exceptions.OBJECT_DOESNT_EXIST, script);
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


        public static void ThrowErrorMsg(string msg, Exceptions errorcode, ParsingScript script, string token = null)
        {
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


            StringBuilder stack = new StringBuilder();
            stack.AppendLine(currentLineNumber.ToString());
            stack.AppendLine(filename);
            stack.AppendLine(line);
            throw new ScriptException(msg + stack.ToString(), ecode);
        }

        private static void ThrowErrorMsg(string msg, string code, Exceptions ecode, int level, int lineStart, int lineEnd, string filename)
        {
            var lineNumber = level > 0 ? lineStart : lineEnd;
            ThrowErrorMsg(msg, code, ecode, lineNumber, filename);
        }

        public static void CheckLegalName(string name, bool checkReserved = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                Utils.ThrowErrorMsg("識別子を空にすることはできません", Exceptions.ILLEGAL_IDENTIFIER, null, name);
            }
            if (checkReserved && Constants.CheckReserved(name))
            {
                Utils.ThrowErrorMsg(name + "は予約語のため使用できません", Exceptions.ITS_RESERVED_NAME, null, name);
            }

            if (!Constants.IDENTIFIER_PATTERN.IsMatch(name))
            {
                Utils.ThrowErrorMsg($"識別子`{name}`には使用できない文字が含まれています", Exceptions.ILLEGAL_IDENTIFIER, null, name);
            }
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
            tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(script.StackTrace);
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
                if (args[i] == null)
                {
                    throw new ScriptException("関数 `" + functionName + "` の`" + i + "`番目の引数が不正です。", Exceptions.INVAILD_ARGUMENT_FUNCTION);
                }
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
            return script != null && script.Package != null && script.Package.ExistsEntry(filename)
                ? script.Package.GetEntryData(filename)
                : fromPackage || !File.Exists(filename) ? throw new FileNotFoundException(null, filename) : File.ReadAllBytes(filename);
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

        public static GetVarFunction ExtractArrayElement(string token, ParsingScript script)
        {
            if (!token.Contains(Constants.START_ARRAY))
            {
                return null;
            }

            ParsingScript tempScript = script.GetTempScript(token);
            Variable result = tempScript.Execute();
            return new GetVarFunction(result);
        }


        public static int GetSafeInt(List<Variable> args, int index, int defaultValue = 0)
        {
            if (args.Count <= index)
            {
                return defaultValue;
            }
            Variable numberVar = args[index];
            return numberVar.AsInt();
        }

        public static string GetSafeString(List<Variable> args, int index, string defaultValue = "")
        {
            return args.Count <= index ? defaultValue : args[index].AsString();
        }
        public static bool GetSafeBool(List<Variable> args, int index, bool defaultValue = false)
        {
            return args.Count <= index ? defaultValue : args[index].AsBool();
        }
        public static Variable GetSafeVariable(List<Variable> args, int index, Variable defaultValue = null)
        {
            return args.Count <= index ? defaultValue : args[index];
        }
        public static double ConvertToDouble(object obj, ParsingScript script = null, bool throwError = true)
        {
            string str = obj.ToString().ToLower();
            if (!CanConvertToDouble(str, out double num) &&
                script != null && str != Constants.END_ARRAY.ToString() && throwError)
            {
                ProcessErrorMsg(str, script);
            }
            return num;
        }

        public static bool CanConvertToDouble(string str, out double num)
        {
            //文字列を小文字に置き換え
            str = str.ToLower();
            if (str.StartsWith("_", StringComparison.Ordinal) || str.EndsWith("_", StringComparison.Ordinal) || str.Contains("_.") || str.Contains("._"))
            {
                throw new ScriptException("数値リテラルの先頭・末尾または小数点の前後にアンダースコア(_)を含めることはできません", Exceptions.INVALID_NUMERIC_REPRESENTATION);
            }
            if (str.Length - str.Replace(".", "").Length > 1)
            {
                throw new ScriptException("数値リテラルで小数点は一度のみ使用できます", Exceptions.INVALID_NUMERIC_REPRESENTATION);
            }
            str = str.Replace("_", "");
            //0xから始まる実数の16進表現を確認します
            try
            {
                if (str.StartsWith("0x", StringComparison.Ordinal))
                {
                    num = Convert.ToInt32(str.Substring(2), 16);
                    return true;
                }
                else if (str.StartsWith("0o", StringComparison.Ordinal))
                {
                    num = Convert.ToInt32(str.Substring(2), 8);
                    return true;
                }
                else if (str.StartsWith("0b", StringComparison.Ordinal))
                {
                    num = Convert.ToInt32(str.Substring(2), 2);
                    return true;
                }
            }
            catch (FormatException)
            {
                throw new ScriptException("無効な数値表現です", Exceptions.INVALID_NUMERIC_REPRESENTATION);
            }
            return double.TryParse(str, NumberStyles.Float,
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

                string msg = entity + " `" + token + "`は定義されていないか、存在しません";

                ThrowErrorMsg(msg, ex, script, str);
            }

        }

        public static string ConvertUnicodeLiteral(string input)
        {
            if (input.Contains("\\"))
            {
                //UTF-16文字コードの置き換え
                foreach (Match match in Constants.UTF16_LITERAL.Matches(input))
                {
                    input = input.Replace(match.Value, ConvertUnicodeToChar(match.Value.TrimStart('\\', 'u')));
                }
                //可変長UTF-16文字コードの置き換え
                foreach (Match match in Constants.UTF16_VARIABLE_LITERAL.Matches(input))
                {
                    input = input.Replace(match.Value, ConvertUnicodeToChar(match.Value.TrimStart('\\', 'x')));
                }
                //UTF-32文字コードの置き換え
                foreach (Match match in Constants.UTF32_LITERAL.Matches(input))
                {
                    input = input.Replace(match.Value, ConvertUnicodeToChar(match.Value.TrimStart('\\', 'U'), false));
                }
            }
            return input;
        }
        private static string ConvertUnicodeToChar(string charCode, bool mode = true)
        {
            if (mode)
            {
                int charCode16 = Convert.ToInt32(charCode, 16);  // 16進数文字列 -> 数値
                char c = Convert.ToChar(charCode16);  // 数値(文字コード) -> 文字
                return c.ToString();
            }
            else
            {
                //UTF-32モード
                int charCode32 = Convert.ToInt32(charCode, 16);  // 16進数文字列 -> 数値
                return char.ConvertFromUtf32(charCode32);
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

            int index = data.IndexOf(".", StringComparison.Ordinal);
            return index < 0 || index >= data.Length - 1 ? 0 : data.Length - index - 1;
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
            return string.IsNullOrWhiteSpace(path) ? GetCurrentDirectory() : Path.GetDirectoryName(path);
        }

        public static string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
