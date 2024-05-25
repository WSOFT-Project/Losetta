using AliceScript.Extra;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace AliceScript
{
    public static partial class Utils
    {
        /// <summary>
        /// 引数の個数を確認し、不足していれば例外を発生させます
        /// </summary>
        /// <param name="args">実際の引数の個数</param>
        /// <param name="expected">必要な引数の個数</param>
        /// <param name="msg">関数名</param>
        /// <param name="exactMatch">ぴったり同じである必要がある場合はtrue</param>
        /// <exception cref="ScriptException">引数が不足している場合に発生する例外</exception>
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
        /// <param name="min">特定範囲より小さな値</param>
        /// <param name="max">特定範囲より大きな値</param>
        /// <param name="needInteger">整数かつInt32の範囲内である必要がある場合はtrue。この値は省略できます。</param>
        /// <param name="script">確認元のスクリプト</param>
        public static void CheckNumInRange(Variable variable, bool needInteger = false, double? min = null, double? max = null, ParsingScript script = null)
        {
            if (!TestNumInRange(variable, needInteger, min, max, script))
            {
                throw new ScriptException($"数値は{(min is not null ? $" {min}以上かつ" : string.Empty)}{(max is not null ? $" {max}以下の" : string.Empty)}{(needInteger ? "整数" : "実数")}である必要があります。", Exceptions.NUMBER_OUT_OF_RANGE, script);
            }
        }

        /// <summary>
        /// 指定した変数が数値を表し、かつ特定範囲内にあるかどうかを表す値を取得します。
        /// </summary>
        /// <param name="variable">確認する変数</param>
        /// <param name="min">特定範囲より小さな値</param>
        /// <param name="max">特定範囲より大きな値</param>
        /// <param name="needInteger">整数かつInt32の範囲内である必要がある場合はtrue。この値は省略できます。</param>
        /// <param name="script">確認元のスクリプト</param>
        /// <returns>指定した変数が数値を表し、かつ特定範囲内にある場合はtrue、それ以外の場合はfalse</returns>
        public static bool TestNumInRange(Variable variable, bool needInteger = false, double? min = null, double? max = null, ParsingScript script = null)
        {
            CheckNumber(variable, script, !(needInteger || max is null || min is null));
            double trueMax = max is null ? (needInteger ? int.MaxValue : double.MaxValue) : max.Value;
            double trueMin = max is null ? (needInteger ? int.MinValue : double.MinValue) : min.Value;
            bool type = !needInteger || variable.Value % 1 == 0.0;
            bool less = variable.Value >= trueMin;
            bool over = variable.Value <= trueMax;
            return type && less && over;
        }
        /// <summary>
        /// 指定した変数が有効な数値であることを確認し、そうでない場合は例外をスローします。
        /// </summary>
        /// <param name="variable">確認する変数</param>
        /// <param name="script">確認元のスクリプト</param>
        /// <param name="acceptNaN">NaNを認める場合はtrue、それ以外の場合はfalse</param>
        public static void CheckNumber(Variable variable, ParsingScript script, bool acceptNaN = false)
        {
            if (variable.Type != Variable.VarType.NUMBER && (acceptNaN || !double.IsNaN(variable.Value)))
            {
                ThrowErrorMsg("型が一致しないか、変換できません。", Exceptions.WRONG_TYPE_VARIABLE, script);
            }
        }
        public static void CheckNotNull(string name, ParserFunction func, ParsingScript script)
        {
            if (func is null)
            {
                string realName = Constants.GetRealName(name);
                ThrowErrorMsg("次の変数または関数は存在しません [" + realName + "]", Exceptions.PROPERTY_OR_METHOD_NOT_FOUND, script);
            }
        }
        public static bool CheckNotNull(object obj, string name, ParsingScript script)
        {
            if (obj is null)
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


        public static void CheckLegalName(string name, bool checkReserved = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                Utils.ThrowErrorMsg("識別子を空にすることはできません", Exceptions.ILLEGAL_IDENTIFIER, null, name);
            }
            if (checkReserved && Constants.CheckReserved(name))
            {
                Utils.ThrowErrorMsg($"識別子`{name}`は予約語のため使用できません", Exceptions.ITS_RESERVED_NAME, null, name);
            }

            if (!Constants.IDENTIFIER_PATTERN.IsMatch(name))
            {
                Utils.ThrowErrorMsg($"識別子`{name}`には使用できない文字が含まれています", Exceptions.ILLEGAL_IDENTIFIER, null, name);
            }
        }

        public static ParsingScript GetTempScript(string str,
            ParsingScript script = null, ParsingScript parentScript = null,
            int parentOffset = 0, AliceScriptClass.ClassInstance instance = null)
        {
            ParsingScript tempScript = new ParsingScript(str);
            tempScript.ScriptOffset = parentOffset;
            if (parentScript is not null)
            {
                tempScript.Char2Line = parentScript.Char2Line;
                tempScript.Filename = parentScript.Filename;
                tempScript.OriginalScript = parentScript.OriginalScript;
            }
            tempScript.ParentScript = script;
            tempScript.ClassInstance = instance;
            tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(script.StackTrace);
            if (script is not null)
            {
                tempScript.Package = script.Package;
                tempScript.Tag = script.Tag;
            }

            return tempScript;
        }

        public static bool ExtractParameterNames(List<Variable> args, string functionName, ParsingScript script)
        {
            if (ParserFunction.GetFunction(functionName, script) is not FunctionBase custFunc)
            {
                return false;
            }

            var realArgs = custFunc.RealArgs;
            for (int i = 0; i < args.Count && i < realArgs.Length; i++)
            {
                if (args[i] is null)
                {
                    throw new ScriptException("関数 `" + functionName + "` の`" + i + "`番目の引数が不正です。", Exceptions.INVALID_ARGUMENT_FUNCTION);
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
            return script is not null && script.Package is not null && script.Package.ExistsEntry(filename)
                ? script.Package.GetEntryData(filename)
                : fromPackage || !File.Exists(filename) ? throw new FileNotFoundException(null, filename) : File.ReadAllBytes(filename);
        }
        public static string GetFileLines(string filename)
        {
            string lines = SafeReader.ReadAllText(filename, out _, out _);
            if (lines is null)
            {
                lines = string.Empty;
            }
            return lines;
        }
        public static string GetFileLines(byte[] data)
        {
            string lines = SafeReader.ReadAllText(data, out _, out _);
            if (lines is null)
            {
                lines = string.Empty;
            }
            return lines;
        }

        public static ValueFunction ExtractArrayElement(string token, ParsingScript script)
        {
            if (!token.Contains(Constants.START_ARRAY))
            {
                return null;
            }

            ParsingScript tempScript = script.GetTempScript(token);
            Variable result = tempScript.Execute();
            return new ValueFunction(result);
        }


        public static int GetSafeInt(List<Variable> args, int index, int defaultValue = 0)
        {
            if (args.Count <= index)
            {
                return defaultValue;
            }
            Variable numberVar = args[index];
            return numberVar.As<int>();
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
            string str = obj.ToString().ToLowerInvariant();
            if (!CanConvertToDouble(str, out double num) &&
                script is not null && str != Constants.END_ARRAY.ToString() && throwError)
            {
                ProcessErrorMsg(str, script);
            }
            return num;
        }

        public static bool CanConvertToDouble(string str, out double num)
        {
            //文字列を小文字に置き換え
            str = str.ToLowerInvariant();
            if (str.StartsWith('_') || str.EndsWith('_') || str.Contains("_.") || str.Contains("._"))
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
                    num = int.Parse(str.AsSpan(2), NumberStyles.HexNumber);
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


        private static readonly char[] separator = new char[] { ',', ':' };

        public static int GetNumberOfDigits(string data, int itemNumber = -1)
        {
            if (itemNumber >= 0)
            {
                string[] vals = data.Split(separator);
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

            int index = data.IndexOf('.');
            return index < 0 || index >= data.Length - 1 ? 0 : data.Length - index - 1;
        }

        public static Span<T> GetSpan<T>(List<T> list)
        {
            return CollectionsMarshal.AsSpan(list);
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
            return string.IsNullOrWhiteSpace(path) ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(path);
        }

    }
}
