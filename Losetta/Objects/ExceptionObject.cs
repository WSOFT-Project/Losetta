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
        public Exceptions ErrorCode { get; set; }
        public ParsingScript MainScript { get; set; }
        public ExceptionObject(string message, Exceptions errorcode, ParsingScript mainScript, string source = null, string helplink = null)
        {
            Message = message;
            ErrorCode = errorcode;
            MainScript = mainScript;
            Source = source;
            HelpLink = helplink;
        }
        public ExceptionObject() { }
        public override string ToString()
        {
            return ErrorCode.ToString() + "(0x" + ((int)ErrorCode).ToString("x3") + ")" + (string.IsNullOrWhiteSpace(Message) ? string.Empty : ": " + Message);
        }
    }
}
