using AliceScript.Binding;
using AliceScript.Parsing;

namespace AliceScript.Objects
{
    /// <summary>
    /// AliceScript内で発生した例外を表すオブジェクト
    /// </summary>
    [AliceObject(Name = "Exception")]
    public class ExceptionObject
    {
        public string Message { get; set; }
        public string HelpLink { get; set; }
        public string Source { get; set; }
        public Exceptions Error { get; set; }
        public int ErrorCode
        {
            get
            {
                return (int)Error;
            }
            set
            {
                Error = (Exceptions)value;
            }
        }
        public ParsingScript MainScript { get; set; }
        public ExceptionObject(string message, Exceptions errorcode, ParsingScript mainScript, string source = null, string helplink = null)
        {
            Message = message;
            Error = errorcode;
            MainScript = mainScript;
            Source = source;
            HelpLink = helplink;
        }
        public ExceptionObject() { }
        public override string ToString()
        {
            return ErrorCode.ToString() + "(0x" + Error.ToString("x3") + ")" + (string.IsNullOrWhiteSpace(Message) ? string.Empty : ": " + Message);
        }
    }
}
