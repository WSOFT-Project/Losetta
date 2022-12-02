using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public partial class Constants
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
        public const char TERNARY_OPERATOR = '?';

        public const string FOR_EACH = ":";
        public const string FOR_IN = "in";
        public const string FOR_OF = "of";
        public const string INFINITY = "Infinity";
        public const string NEG_INFINITY = "-Infinity";
        public const string ISFINITE = "IsFinite";
        public const string ISNAN = "IsNaN";
        public const string NULL = "null";
        public const string NAN = "NaN";
        public const string UNDEFINED = "undefined";

        public const string ASSIGNMENT = "=";
        public const string AND = "&&";
        public const string OR = "||";
        public const string NOT = "!";
        public const string INCREMENT = "++";
        public const string DECREMENT = "--";
        public const string EQUAL = "==";
        public const string NOT_EQUAL = "!=";
        public const string LESS = "<";
        public const string LESS_EQ = "<=";
        public const string GREATER = ">";
        public const string GREATER_EQ = ">=";
        public const string ADD_ASSIGN = "+=";
        public const string SUBT_ASSIGN = "-=";
        public const string MULT_ASSIGN = "*=";
        public const string DIV_ASSIGN = "/=";

        public const string BREAK = "break";
        public const string CASE = "case";
        public const string CATCH = "catch";
        public const string CANCEL = "cancel_operation";
        public const string COMMENT = "//";
        public const string CONTINUE = "continue";
        public const string DEFAULT = "default";
        public const string DO = "do";
        public const string ELSE = "else";
        public const string ELSE_IF = "elif";
        public const string FOR = "for";
        public const string FUNCTION = "function";
        public const string CLASS = "class";
        public const string ENUM = "enum";
        public const string IF = "if";
        public const string INCLUDE = "include";
        public const string IMPORT = "import";
        public const string NEW = "new";
        public const string RETURN = "return";
        public const string SWITCH = "switch";
        public const string THIS = "this";
        public const string THROW = "throw";
        public const string TRY = "try";
        public const string TYPE = "type";
        public const string TYPE_OF = "typeOf";
        public const string WHILE = "while";

        public const string TRUE = "true";
        public const string FALSE = "false";

        public const string ADD = "add";
        public const string ADD_RANGE = "AddRange";
        public const string ADD_UNIQUE = "addunique";
        public const string ADD_TO_HASH = "AddToHash";
        public const string ADD_ALL_TO_HASH = "AddAllToHash";
        public const string CANCEL_RUN = "CancelRun";
        public const string CHECK_LOADER_MAIN = "LoaderMain";
        public const string CONST = "const";
        public const string CONTAINS = "contains";
        public const string CURRENT_PATH = "CurrentPath";
        public const string CLONE = "Clone";
        public const string DEEP_CLONE = "DeepClone";
        public const string DEFINE_LOCAL = "DefineLocal";
        public const string EXIT = "exit";
        public const string FIND_INDEX = "FindIndex";
        public const string FLOOR = "floor";
        public const string GET_COLUMN = "GetColumn";
        public const string GET_PROPERTIES = "GetPropertyStrings";
        public const string GET_PROPERTY = "GetProperty";
        public const string GET_KEYS = "GetKeys";
        public const string LOCK = "lock";
        public const string NAMESPACE = "Namespace";
        public const string ON_EXCEPTION = "OnException";
        public const string OBJECT_PROPERTIES = "Properties";
        public const string OBJECT_TYPE = "Type";
        public const string POINTER = "->";
        public const string POINTER_REF = "&";
        public const string PRINT = "print";
        public const string REGEX = "Regex";
        public const string REMOVE = "RemoveItem";
        public const string REMOVE_AT = "RemoveAt";
        public const string REMOVE_RANGE = "RemoveRange";
        public const string SET_PROPERTY = "SetProperty";
        public const string SHOW = "show";
        public const string SIGNAL = "signal";
        public const string SINGLETON = "singleton";
        public const string SIZE = "Size";
        public const string THREAD_ID = "threadid";
        public const string TOKENIZE_LINES = "TokenizeLines";
        public const string TOKEN_COUNTER = "CountTokens";
        public const string TO_STRING = "string";
        public const string VAR = "var";
        public const string WAIT = "wait";

        public const string ADD_DATA = "AddDataToCollection";
        public const string COLLECT_DATA = "StartCollectingData";
        public const string GET_DATA = "GetCollectedData";

        // プロパティ
        public const string EMPTY_NULL = "EmptyOrNull";
        public const string EMPTY_WHITE = "EmptyOrWhite";
        public const string ENDS_WITH = "EndsWith";
        public const string EQUALS = "Equals";
        public const string FIRST = "First";
        public const string FOREACH = "ForEach";
        public const string INDEX_OF = "IndexOf";
        public const string JOIN = "Join";
        public const string KEYS = "Keys";
        public const string LAST = "Last";
        public const string LENGTH = "Length";
        public const string LOWER = "Lower";
        public const string REMOVE_ITEM = "Remove";
        public const string REPLACE = "Replace";
        public const string REPLACE_TRIM = "ReplaceAndTrim";
        public const string REVERSE = "Reverse";
        public const string SORT = "Sort";
        public const string SPLIT = "Split";
        public const string STRING = "String";
        public const string STARTS_WITH = "StartsWith";
        public const string SUBSTRING = "Substring";
        public const string TOKENIZE = "Tokenize";
        public const string TRIM = "Trim";
        public const string TRIM_START = "TrimStart";
        public const string TRIM_END = "TrimEnd";
        public const string UPPER = "Upper";
        public const string INSERT = "Insert";
        public const string INSERT_RANGE = "InsertRange";

        public const string LABEL_OPERATOR = ":";
        public const string GOTO = "goto";
        public const string GOSUB = "gosub";

        public const string OBJECT_DEFPROP = "Object.defineProperty";


        public const string PROP_TO_STRING = "ToString";

        public static string END_ARG_STR = END_ARG.ToString();
        public static string NULL_ACTION = END_ARG.ToString();

        public static string[] OPER_ACTIONS = { "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "->", ":", "??="};
        public static string[] MATH_ACTIONS = { "===", "!==",
                                                "&&", "||", "==", "!=", "<=", ">=", "++", "--", "**",
                                                "%", "*", "/", "+", "-", "^", "&", "|", "<", ">", "=","is","as","??"};

        public static string[] ACTIONS = (OPER_ACTIONS.Union(MATH_ACTIONS)).ToArray();

        public static string[] CORE_OPERATORS = (new List<string> { TRY, FOR, WHILE }).ToArray();

        // ICEファイルのマーク(ASCIIでI,C,Eとバージョン(1))
        public static byte[] PACKAGE_MAGIC_NUMBER = { 0x49, 0x43, 0x45,0x01 };
        // パッケージマニフェストファイルの名前
        public const string PACKAGE_MANIFEST_FILENAME = "manifest.xml";

        public static char[] TERNARY_SEPARATOR = { ':' };
        public static char[] NEXT_ARG_ARRAY = NEXT_ARG.ToString().ToCharArray();
        public static char[] END_ARG_ARRAY = END_ARG.ToString().ToCharArray();
        public static char[] END_ARRAY_ARRAY = END_ARRAY.ToString().ToCharArray();
        public static char[] END_LINE_ARRAY = END_LINE.ToString().ToCharArray();
        public static char[] FOR_ARRAY = (END_ARG_STR + FOR_EACH).ToCharArray();
        public static char[] QUOTE_ARRAY = QUOTE.ToString().ToCharArray();

        public static char[] COMPARE_ARRAY = "<>=)".ToCharArray();
        public static char[] IF_ARG_ARRAY = "&|)".ToCharArray();
        public static char[] END_PARSE_ARRAY = { SPACE, END_STATEMENT, END_ARG, END_GROUP, '\n' };
        public static char[] NEXT_OR_END_ARRAY = { NEXT_ARG, END_ARG, END_GROUP, END_STATEMENT, SPACE };
        public static char[] NEXT_OR_END_ARRAY_EXT = { NEXT_ARG, END_ARG, END_GROUP, END_ARRAY, END_STATEMENT, SPACE };

        public static string TOKEN_SEPARATION_STR = "<>=+-*/%&|^,!()[]{}\t\n;: ";
        public static char[] TOKEN_SEPARATION = TOKEN_SEPARATION_STR.ToCharArray();
        public static char[] TOKENS_SEPARATION = ",;)".ToCharArray();

        // 関数呼び出し時に丸括弧が不要な関数
        public static List<string> FUNCT_WITH_SPACE = new List<string>
        {
            CLASS,
            FUNCTION, NAMESPACE, NEW, PRINT,SHOW,
        };
        //関数呼び出し時に丸括弧が不要な関数。ただしこれらの関数の引数は一つのみである必要があります。
        public static List<string> FUNCT_WITH_SPACE_ONCE = new List<string>
        {
            CASE, RETURN, THROW, TYPE_OF, VAR,CONST
        };

        // 言語構造の予約。これらを演算したり返すことは無意味
        public static List<string> CONTROL_FLOW = new List<string>
        {
            BREAK, CATCH, CLASS, CONTINUE, ELSE, ELSE_IF, ELSE, FOR,FOREACH, FUNCTION, IF, INCLUDE, NEW,IMPORT,
            RETURN, THROW, TRY, WHILE
        };


        //配列添え字演算子を使用できる変数の型
        public static List<Variable.VarType> CAN_GET_ARRAYELEMENT_VARIABLE_TYPES = new List<Variable.VarType>()
        {
            Variable.VarType.ARRAY,Variable.VarType.DELEGATE,Variable.VarType.STRING
        };
        //予約語
        public static List<string> RESERVED = new List<string>
        {
            BREAK, CONTINUE, CLASS, NEW, FUNCTION, IF, ELSE, ELSE_IF, INCLUDE,IMPORT, FOR,FOREACH, WHILE,
            RETURN, THROW, TRY, CATCH, COMMENT, TRUE, FALSE, TYPE,
            ASSIGNMENT, AND, OR, EQUAL, NOT_EQUAL, LESS, LESS_EQ, GREATER, GREATER_EQ,
            ADD_ASSIGN, SUBT_ASSIGN, MULT_ASSIGN, DIV_ASSIGN,
            SWITCH, CASE, DEFAULT, NAN, UNDEFINED,NULL,
            NEXT_ARG.ToString(), START_GROUP.ToString(), END_GROUP.ToString(), END_STATEMENT.ToString()
        };
        //インタプリタに最初から定義される定数
        public static Dictionary<string, Variable> CONSTS = new Dictionary<string, Variable> {
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
            //定義されていないことを表します
            { UNDEFINED,new Variable(Variable.VarType.UNDEFINED)},
            //ループを抜けます
            {BREAK,new Variable(Variable.VarType.BREAK) },
            //ループを次に進めます
            {CONTINUE,new Variable(Variable.VarType.CONTINUE) },
            {"string", Variable.AsType(Variable.VarType.STRING) },
            {"number",Variable.AsType(Variable.VarType.NUMBER) },
            {"array",Variable.AsType(Variable.VarType.ARRAY) },
            {"bytes",Variable.AsType(Variable.VarType.BYTES) },
            {"object",Variable.AsType(Variable.VarType.OBJECT) },
            {"enum",Variable.AsType(Variable.VarType.ENUM) },
            {"pointer",Variable.AsType(Variable.VarType.POINTER) },
            {"delegate",Variable.AsType(Variable.VarType.DELEGATE) },
            {"bool",Variable.AsType(Variable.VarType.BOOLEAN) },
            {"type",Variable.AsType(Variable.VarType.TYPE) }

        };
        //型指定修飾子
        public static List<string> TYPE_MODIFER = new List<string>{
             "string","number","array","bytes","object","enum","pointer","delegate","bool","type"
        };
        //算術演算子
        public static List<string> ARITHMETIC_EXPR = new List<string>
        {
            "*", "*=" , "+", "+=" , "-", "-=", "/", "/=", "%", "%=", ">", "<", ">=", "<="
        };

        public const int INDENT = 2;
        public const int DEFAULT_FILE_LINES = 20;
        public const int MAX_CHARS_TO_SHOW = 45;

        static Dictionary<string, string> s_realNames = new Dictionary<string, string>();

        public static string ConvertName(string name)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name) || name[0] == QUOTE || name[0] == QUOTE1)
            {
                return name;
            }

            string lower = name.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (name == lower || CONTROL_FLOW.Contains(lower))
            { // Do not permit using key words with no case, like IF, For
                return name;
            }

            s_realNames[lower] = name;
            return lower;
        }

        public static bool CheckReserved(string name)
        {
            return Constants.RESERVED.Contains(name);
        }

        public static string GetRealName(string name)
        {
            name = name.Trim();
            string realName;
            if (!s_realNames.TryGetValue(name, out realName))
            {
                return name;
            }
            return realName;
        }
        
        public static string TypeToString(Variable.VarType type)
        {
            switch (type)
            {
                case Variable.VarType.NUMBER:   return "NUMBER";
                case Variable.VarType.STRING:   return "STRING";
                case Variable.VarType.ARRAY_STR:
                case Variable.VarType.ARRAY_NUM:
                case Variable.VarType.ARRAY:    return "ARRAY";
                case Variable.VarType.MAP_STR:
                case Variable.VarType.MAP_NUM:  return "MAP";
                case Variable.VarType.OBJECT:   return "OBJECT";
                case Variable.VarType.BREAK:    return "BREAK";
                case Variable.VarType.CONTINUE: return "CONTINUE";
                case Variable.VarType.DELEGATE: return "DELEGATE";
                case Variable.VarType.BOOLEAN:  return "BOOLEAN";
                case Variable.VarType.BYTES:    return "BYTES";
                case Variable.VarType.TYPE:     return "TYPE";
                case Variable.VarType.UNDEFINED: return "UNDEFINED";
                default: return "NONE";
            }
        }
        public static bool TryParseType(string text,out Variable.VarType type)
        {
            text = text.ToUpper();
            switch (text)
            {
                case "INT":
                case "FLOAT":
                case "DOUBLE":
                case "NUMBER": type= Variable.VarType.NUMBER;break;
                case "CHAR":
                case "STRING": type= Variable.VarType.STRING; break;
                case "LIST<INT>":
                case "LIST<DOUBLE>": type= Variable.VarType.ARRAY_NUM; break;
                case "LIST<STRING>": type= Variable.VarType.ARRAY_STR; break;
                case "MAP<INT>":
                case "MAP<STRING,INT>":
                case "MAP<DOUBLE>":
                case "MAP<STRING,DOUBLE>": type= Variable.VarType.MAP_NUM; break;
                case "MAP<STRING>":
                case "MAP<STRING,STRING>": type= Variable.VarType.MAP_STR; break;
                case "TUPLE":
                case "ARRAY": type= Variable.VarType.ARRAY; break;
                case "BOOL":
                case "BOOLEAN": type= Variable.VarType.BOOLEAN; break;
                case "BREAK": type= Variable.VarType.BREAK; break;
                case "CONTINUE": type= Variable.VarType.CONTINUE; break;
                case "DELEGATE": type= Variable.VarType.DELEGATE; break;
                case "VARIABLE": type= Variable.VarType.VARIABLE; break;
                case "TYPE": type= Variable.VarType.TYPE; break;
                default: type = Variable.VarType.NONE; return false;
            }
            return true;
        }
        public static Variable.VarType StringToType(string type)
        {
            Variable.VarType ptype;
            TryParseType(type, out ptype);
            return ptype;
        }
    }
}
