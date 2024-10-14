using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AliceScript.Objects;

namespace AliceScript
{
    /// <summary>
    /// AliceScriptで使用される定数。この定義を変更することでカスタマイズ可能です。
    /// </summary>
    public static partial class Constants
    {
        public const char ATMARK = '@';
        public const char START_ARG = '(';
        public const char END_ARG = ')';
        public const char START_ARRAY = '[';
        public const char END_ARRAY = ']';
        public const char END_LINE = '\n';
        public const char NEXT_ARG = ',';
        public const char QUOTE = '"';
        public const char QUOTE1 = '\'';
        public const char SPACE = ' ';
        public const char START_GROUP = '{';
        public const char END_GROUP = '}';
        public const char VAR_START = '$';
        public const char END_STATEMENT = ';';
        public const char CONTINUE_LINE = '\\';
        public const char EMPTY = '\0';
        public const char DESTRUCTION = '_';
        public const char TERNARY_OPERATOR = '?';
        public const char DOLLER = '$';
        public const char PLUS = '+';
        public const char MINUS = '-';
        public const char BITWISE_NOT = '~'; 

        public const char QUOTE_IN_LITERAL = '\ufdd2';
        public const char QUOTE1_IN_LITERAL = '\ufdd1';
        public const char USER_CANT_USE_FUNCTION_PREFIX = '\ufdd3';
        public const char USER_CANT_USE_VARIABLE_PREFIX = '\ufdd4';
        public const char ANNOTATION_FUNCTION_REFIX = '\ufdd5';

        public const string RANGE = "..";
        public const string AS = "as ";
        public const string IS = "is ";
        public const string IS_NOT = "is not ";
        public const string FOR_IN = " in ";
        public const string INFINITY = "Infinity";
        public const string NEG_INFINITY = "-Infinity";
        public const string ISFINITE = "IsFinite";
        public const string ISNAN = "IsNaN";
        public const string NULL = "null";
        public const string NAN = "NaN";
        public const string UNDEFINED = "undefined";

        public const string ASSIGNMENT = "=";
        public const string ARROW = "=>";
        public const string AND = "&&";
        public const string OR = "||";
        public const string INCREMENT = "++";
        public const string DECREMENT = "--";
        public const string EQUAL = "==";
        public const string NOT_EQUAL = "!=";
        public const string NULL_OP = "??";
        public const string LESS = "<";
        public const string LESS_EQ = "<=";
        public const string GREATER = ">";
        public const string GREATER_EQ = ">=";

        public const string NOT = "!";
        public const string EXPONENTIATION = "**";
        public const string LEFT_SHIFT = "<<";
        public const string RIGHT_SHIFT = ">>";
        public const string SPREAD = "...";

        public const string BREAK = "break";
        public const string CASE = "case";
        public const string CATCH = "catch";
        public const string COMMENT = "//";
        public const string CONTINUE = "continue";
        public const string DEFAULT = "default";
        public const string DO = "do";
        public const string EXTERNAL = "extern";
        public const string ELSE = "else";
        public const string ELSE_IF = "elif";
        public const string FOR = "for";
        public const string FINALLY = "finally";
        public const string CLASS = "class";
        public const string ENUM = "enum";
        public const string IF = "if";
        public const string IMPORT = "import";
        public const string INCLUDE = "include";
        public const string NEW = "new";
        public const string RETURN = "return";
        public const string SWITCH = "switch";
        public const string THIS = "this";
        public const string THROW = "throw";
        public const string TRY = "try";
        public const string TYPE = "type";
        public const string WHILE = "while";
        public const string WHEN = "when";

        public const string TRUE = "true";
        public const string FALSE = "false";

        public const string REF = "ref";
        public const string PARAMS = "params";
        public const string REQUIRES = "requires";
        public const string ENSURES = "ensures";

        public const string ADD = "add";
        public const string ADD_TO_HASH = "AddToHash";
        public const string DEFINE_LOCAL = "DefineLocal";
        public const string GET_PROPERTY = "GetProperty";
        public const string GET_KEYS = "GetKeys";
        public const string NAMESPACE = "Namespace";
        public const string OBJECT_PROPERTIES = "Properties";
        public const string OBJECT_TYPE = "Type";

        public const string GOTO = "goto";
        public const string GOSUB = "gosub";

        public const string DEBUG = "DEBUG";

        /// <summary>
        /// このプログラミング言語の名前
        /// </summary>
        public const string LANGUAGE = "AliceScript";

        public static readonly Version VERSION = new Version(3, 0);

        public const string UTF8_LITERAL_PREFIX = "u8";

        public const string PROP_TO_STRING = "ToString";

        public static char[] EMPTY_AND_WHITE = new char[] { ' ', '\0', '\n', '\r', '\t' };

        public static readonly string END_ARG_STR = END_ARG.ToString();
        public static readonly string NULL_ACTION = END_ARG.ToString();

        public static readonly string[] OPER_ACTIONS = {  ARROW };
        public static readonly string[] MATH_ACTIONS = { "===", "!==",
                                                AND, OR, EQUAL,NOT_EQUAL, LESS_EQ, GREATER_EQ, INCREMENT,DECREMENT,EXPONENTIATION,LEFT_SHIFT,RIGHT_SHIFT,
                                                "%", "*", "/", "+", "-", "^", "&", "|", "<", ">", "=",":",NULL_OP,AS,IS_NOT,IS, RANGE};

        /// <summary>
        /// 単項前置演算子(1文字のもの)
        /// </summary>
        public static readonly char[] PRE_SINGLE_SIZE_ACTIONS = { PLUS, MINUS, BITWISE_NOT };
        /// <summary>
        /// 単項前置演算子(2文字のもの)
        /// </summary>
        public static readonly string[] PRE_DOUBLE_SIZE_ACTIONS = { INCREMENT, DECREMENT, RANGE};
        public static readonly string[] ACTIONS = OPER_ACTIONS.Union(MATH_ACTIONS).ToArray();

        public static readonly string[] CORE_OPERATORS = { TRY, FOR, WHILE };

        /// <summary>
        /// ICEファイルのマーク(ASCIIでI,C,Eとバージョン(1))
        /// </summary>
        public static readonly byte[] PACKAGE_MAGIC_NUMBER = { 0x49, 0x43, 0x45, 0x01 };
        /// <summary>
        /// DLLファイルのマーク(ASCIIでM,Z)
        /// </summary>
        public static readonly byte[] DLL_MAGIC_NUMBER = { 0x4d, 0x5a };
        /// <summary>
        /// ZIPファイルのマーク(ASCIIでP,K)
        /// </summary>
        public static readonly byte[] ZIP_MAGIC_NUMBER = { 0x50, 0x4b };
        /// <summary>
        /// パッケージマニフェストファイルの名前
        /// </summary>
        public const string PACKAGE_MANIFEST_FILENAME = "manifest.xml";

        public static readonly char[] TERNARY_SEPARATOR = { ':' };
        public static readonly char[] NEXT_ARG_ARRAY = NEXT_ARG.ToString().ToCharArray();
        public static readonly char[] END_ARG_ARRAY = END_ARG.ToString().ToCharArray();
        public static readonly char[] END_ARRAY_ARRAY = END_ARRAY.ToString().ToCharArray();
        public static readonly char[] END_LINE_ARRAY = END_LINE.ToString().ToCharArray();
        public static readonly char[] QUOTE_ARRAY = QUOTE.ToString().ToCharArray();

        public static readonly char[] COMPARE_ARRAY = "<>=)".ToCharArray();
        public static readonly char[] IF_ARG_ARRAY = "&|)".ToCharArray();
        public static readonly char[] END_PARSE_ARRAY = { SPACE, END_STATEMENT, END_ARG, END_GROUP, '\n', '?' };
        public static readonly char[] NEXT_OR_END_ARRAY = { NEXT_ARG, END_ARG, END_GROUP, END_STATEMENT, SPACE };
        public static readonly char[] NEXT_OR_END_ARRAY_EXT = { NEXT_ARG, END_ARG, END_GROUP, END_ARRAY, END_STATEMENT, SPACE };

        public static readonly string TOKEN_START = "(\"\'[{";
        public static readonly string TOKEN_END = ")\"\']}";
        public static readonly string TOKEN_SEPARATION_STR = "<>=+-*/%&|^,!()[]{}\t\n;: ";
        public static readonly char[] TOKEN_SEPARATION = TOKEN_SEPARATION_STR.ToCharArray();
        public static readonly string TOKEN_SEPARATION_ANDEND_STR = TOKEN_SEPARATION_STR + "\0";
        public static readonly string TOKENS_SEPARATION_STR = ",;)";
        public static readonly char[] TOKENS_SEPARATION = TOKENS_SEPARATION_STR.ToCharArray();

        /// <summary>
        /// ソースコード上では無視される文字
        /// </summary>
        public const string IGNORE_CHARS = "\t\r";

        /// <summary>
        /// パース中の言語構造が所属する名前空間
        /// </summary>
        public static readonly string PARSING_NAMESPACE = TOP_API_NAMESPACE + ".Parsing";

        /// <summary>
        /// グローバルの名前空間名
        /// </summary>
        public const string TOP_NAMESPACE = ":Global:";

        /// <summary>
        /// APIが使用する最上位の名前空間
        /// </summary>
        public const string TOP_API_NAMESPACE = "Alice";

        /// <summary>
        /// 変数・定数・関数名などの識別子がとるパターン
        /// </summary>
        public static readonly Regex IDENTIFIER_PATTERN = new Regex("^[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\ufdd3\\ufdd4][\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Pc}\\p{Nd}\\p{Cf}\\.]*$", RegexOptions.Compiled);

        /// <summary>
        /// 複合代入式がとるパターン
        /// </summary>
        public static readonly Regex COMPOUND_ASSIGN_PATTERN = new Regex(@"(.*)([\+\-\\\*\/\%\^\&\|\?])=(.*)", RegexOptions.Compiled);

        /// <summary>
        /// UTF16表現がとるパターン
        /// </summary>
        public static readonly Regex UTF16_LITERAL = new Regex(@"[^\\]\\u[0-9a-fA-F]{4}", RegexOptions.Compiled);

        /// <summary>
        /// 可変長UTF16表現がとるパターン
        /// </summary>
        public static readonly Regex UTF16_VARIABLE_LITERAL = new Regex(@"[^\\]\\x[0-9a-fA-F]{1,4}", RegexOptions.Compiled);

        /// <summary>
        /// UTF32表現がとるパターン
        /// </summary>
        public static readonly Regex UTF32_LITERAL = new Regex(@"[^\\]\\U[0-9a-fA-F]{8}", RegexOptions.Compiled);

        /// <summary>
        /// return式がとるパターン
        /// </summary>
        public static readonly Regex RETURN_PATTERN = new Regex("return([\\s\\S]*?);", RegexOptions.Compiled);

        // キーワード
        public const string PUBLIC = "public";
        public const string PRIVATE = "private";
        public const string PROTECTED = "protected";
        public const string VAR = "var";
        public const string CONST = "const";
        public const string VIRTUAL = "virtual";
        public const string OVERRIDE = "override";
        public const string COMMAND = "command";
        public const string READONLY = "readonly";
        public const string EXTENSION = "extonly";
        public const string FUNCTION = "function";


        /// <summary>
        /// 型指定修飾子
        /// </summary>
        public static readonly HashSet<string> TYPE_MODIFER = new HashSet<string>{
             "string","number","array","bytes","object","enum","delegate","bool","variable","void",
              "string?","number?","array?","bytes?","object?","enum?","delegate?","bool?","variable?","var"
        };
        /// <summary>
        /// AliceScriptのキーワード
        /// </summary>
        public static readonly HashSet<string> KEYWORD = TYPE_MODIFER.Union(new string[] { PUBLIC, PRIVATE, PROTECTED, VAR, CONST, FUNCTION, VIRTUAL, OVERRIDE, COMMAND, REF, READONLY, EXTENSION, NEW }).ToHashSet();

        // シンボル
        public const string LIBRARY_IMPORT = "libimport";
        public const string NET_IMPORT = "netimport";
        public const string UNNEED_VAR = "unneed_var";
        public const string RESET_DEFINES = "reset_defines";
        public const string OBSOLETE = "obsolete";
        //includeしたファイルにもシンボルを引き継ぐ
        public const string FOLLOW_INCLUDE = "follow_include";
        //varキーワードの型推論を有効にする
        public const string TYPE_INFERENCE = "type_inference";
        public const string FALL_THROUGH = "fall_through";
        public const string CHECK_BREAK_WHEN_CASE = "check_break_when_case";
        public const string ENABLE_USING = "enable_using";
        public const string ENABLE_IMPORT = "enable_import";
        public const string ENABLE_INCLUDE = "enable_include";
        public const string NULLABLE = "nullable";
        //最上位のスクリプトへのアクセスを拒否
        public const string DENY_TO_TOPLEVEL_SCRIPT = "deny_to_toplevel_script";

        /// <summary>
        /// 他にUrlがなかった場合のヘルプUrl
        /// </summary>
        public const string HELP_LINK = "https://a.wsoft.ws/alice/exceptions/0x";


        /// <summary>
        /// 関数呼び出し時に丸括弧が不要な関数の名前
        /// </summary>
        public static readonly HashSet<string> FUNCT_WITH_SPACE = new HashSet<string>
        {
            CLASS,
            FUNCTION, NAMESPACE, NEW
        };
        /// <summary>
        /// 関数呼び出し時に丸括弧が不要な関数。ただしこれらの関数の引数は一つのみである必要があります。
        /// </summary>
        public static readonly HashSet<string> FUNCT_WITH_SPACE_ONCE = new HashSet<string>
        {
            CASE, RETURN, THROW
        };

        /// <summary>
        /// 言語構造の関数名
        /// </summary>
        public static readonly HashSet<string> CONTROL_FLOW = new HashSet<string>
        {
            BREAK, CATCH, CLASS, CONTINUE, ELSE, ELSE_IF, ELSE, FOR,"foreach", FUNCTION, IF, INCLUDE, NEW,IMPORT,
            RETURN, THROW, TRY, WHILE
        };

        /// <summary>
        /// 配列添え字演算子を使用できる変数の型
        /// </summary>
        public static readonly HashSet<Variable.VarType> CAN_GET_ARRAYELEMENT_VARIABLE_TYPES = new HashSet<Variable.VarType>()
        {
            Variable.VarType.ARRAY,Variable.VarType.DELEGATE,Variable.VarType.STRING
        };
        /// <summary>
        /// AliceScriptから参照できる定数
        /// </summary>
        public static readonly Dictionary<string, Variable> CONSTS = new Dictionary<string, Variable>
        {
            //Trueを表します
            { TRUE,Variable.True},
            //Falseを表します
            { FALSE,Variable.False},
            //nullを表します
            { NULL,Variable.EmptyInstance},
            //無限を表します
            { INFINITY,new Variable(double.PositiveInfinity)},
            //負の無限を表します
            { NEG_INFINITY,new Variable(double.NegativeInfinity)},
            //非数を表します
            {NAN,new Variable(double.NaN) },
            //定義されていないことを表します
            { UNDEFINED,new Variable(Variable.VarType.UNDEFINED)},
            //ループを抜けます
            {BREAK,new Variable(Variable.VarType.BREAK) },
            //ループを次に進めます
            {CONTINUE,new Variable(Variable.VarType.CONTINUE) },
            // 配列の全体を表します
            {RANGE, new Variable(new RangeStruct(0))},
            {"string", Variable.AsType(Variable.VarType.STRING) },
            {"number",Variable.AsType(Variable.VarType.NUMBER) },
            {"bytes",Variable.AsType(Variable.VarType.BYTES) },
            {"object",Variable.AsType(Variable.VarType.OBJECT) },
            {"enum",Variable.AsType(Variable.VarType.ENUM) },
            {"delegate",Variable.AsType(Variable.VarType.DELEGATE) },
            {"bool",Variable.AsType(Variable.VarType.BOOLEAN) },

        };
        /// <summary>
        /// 算術演算子
        /// </summary>
        public static readonly HashSet<string> ARITHMETIC_EXPR = new HashSet<string>
        {
            "*", "*=" , "+", "+=" , "-", "-=", "/", "/=", "%", "%=", ">", "<", ">=", "<="
        };

        public const int INDENT = 2;
        public const int DEFAULT_FILE_LINES = 20;
        public const int MAX_CHARS_TO_SHOW = 45;
        private static readonly Dictionary<string, string> s_realNames = new Dictionary<string, string>();

        public static string ConvertName(string name)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name) || name[0] == QUOTE || name[0] == QUOTE1)
            {
                return name;
            }

            string lower = name.ToLowerInvariant();
            if (name == lower || CONTROL_FLOW.Contains(lower))
            { // Do not permit using key words with no case, like IF, For
                return name;
            }

            s_realNames[lower] = name;
            return lower;
        }

        public static bool CheckReserved(string name)
        {
            return Constants.KEYWORD.Contains(name);
        }

        public static string GetRealName(string name)
        {
            name = name.Trim();
            return !s_realNames.TryGetValue(name, out string realName) ? name : realName;
        }

        public static string TypeToString(Variable.VarType type)
        {
            switch (type)
            {
                case Variable.VarType.NUMBER: return "NUMBER";
                case Variable.VarType.STRING: return "STRING";
                case Variable.VarType.ARRAY_STR:
                case Variable.VarType.ARRAY_NUM:
                case Variable.VarType.ARRAY: return "ARRAY";
                case Variable.VarType.MAP_STR:
                case Variable.VarType.MAP_NUM: return "MAP";
                case Variable.VarType.OBJECT: return "OBJECT";
                case Variable.VarType.BREAK: return "BREAK";
                case Variable.VarType.CONTINUE: return "CONTINUE";
                case Variable.VarType.DELEGATE: return "DELEGATE";
                case Variable.VarType.BOOLEAN: return "BOOLEAN";
                case Variable.VarType.BYTES: return "BYTES";
                case Variable.VarType.UNDEFINED: return "UNDEFINED";
                case Variable.VarType.VOID: return "VOID";
                default: return "NONE";
            }
        }
        public static bool TryParseType(string text, out Variable.VarType type)
        {
            text = text.ToUpperInvariant();
            switch (text)
            {
                case "INT":
                case "FLOAT":
                case "DOUBLE":
                case "NUMBER": type = Variable.VarType.NUMBER; break;
                case "CHAR":
                case "STRING": type = Variable.VarType.STRING; break;
                case "LIST<INT>":
                case "LIST<DOUBLE>": type = Variable.VarType.ARRAY_NUM; break;
                case "LIST<STRING>": type = Variable.VarType.ARRAY_STR; break;
                case "MAP<INT>":
                case "MAP<STRING,INT>":
                case "MAP<DOUBLE>":
                case "MAP<STRING,DOUBLE>": type = Variable.VarType.MAP_NUM; break;
                case "MAP<STRING>":
                case "MAP<STRING,STRING>": type = Variable.VarType.MAP_STR; break;
                case "TUPLE":
                case "ARRAY": type = Variable.VarType.ARRAY; break;
                case "BOOL":
                case "BOOLEAN": type = Variable.VarType.BOOLEAN; break;
                case "BREAK": type = Variable.VarType.BREAK; break;
                case "CONTINUE": type = Variable.VarType.CONTINUE; break;
                case "DELEGATE": type = Variable.VarType.DELEGATE; break;
                case "VARIABLE": type = Variable.VarType.VARIABLE; break;
                case "VOID": type = Variable.VarType.VOID; break;
                default: type = Variable.VarType.VARIABLE; return false;
            }
            return true;
        }
        public static Variable.VarType StringToType(string type)
        {
            TryParseType(type, out Variable.VarType ptype);
            return ptype;
        }
        public static Type InvokeStringToType(string typeStr)
        {
            if (typeStr.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
            {
                return InvokeStringToType(typeStr.Substring(0, typeStr.Length - 2)).MakeArrayType();
            }
            switch (typeStr.ToUpperInvariant())
            {
                case "VOID":
                    return typeof(void);
                case "HDC":
                case "HWND":
                case "HANDLE":
                case "INTPTR":
                    return typeof(nint);
                case "UINTPTR":
                    return typeof(nuint);
                case "INT8":
                case "SCHAR":
                case "SBYTE":
                    return typeof(sbyte);
                case "UCHAR":
                case "CHAR":
                case "BYTE":
                case "BOOLEAN":
                    return typeof(byte);
                case "INT16":
                case "SHORT":
                    return typeof(short);
                case "UINT16":
                case "USHORT":
                case "WORD":
                    return typeof(ushort);
                case "INT32":
                case "INT":
                case "LONG32":
                case "LONG":
                    return typeof(int);
                case "UINT":
                case "UINT32":
                case "ULONG32":
                case "ULONG":
                case "DWORD":
                    return typeof(uint);
                case "INT64":
                case "LONG64":
                case "LONGLONG":
                    return typeof(long);
                case "UINT64":
                case "ULONG64":
                case "ULONGLONG":
                    return typeof(ulong);
                case "BOOL":
                    return typeof(bool);
                case "LPTSTR":
                case "LPCSTR":
                case "LPCTSTR":
                case "LPCWSTR":
                case "STRING":
                    return typeof(string);
                case "LPSTR":
                case "LPWSTR":
                case "STRINGBUILDER":
                    return typeof(StringBuilder);
                case "FLOAT":
                case "SINGLE":
                    return typeof(float);
                case "DOUBLE":
                    return typeof(double);
                default:
                    {
                        // 関数のカンマと被るため[System.Console+System.Console]のようにする
                        typeStr = typeStr.Replace('+', ',');
                        return Type.GetType(typeStr, false, true);
                    }
            }
        }
        public static Type[] InvokeStringToType(string[] typeStrs)
        {
            var types = new List<Type>();

            foreach (string typeStr in typeStrs)
            {
                types.Add(InvokeStringToType(typeStr));
            }

            return types.ToArray();
        }
    }
}
