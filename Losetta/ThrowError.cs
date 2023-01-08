using System;

namespace AliceScript
{
    public class ThrowErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public ParsingScript Script { get; set; }
        public ParsingException Exception { get; set; }
        public Exceptions ErrorCode { get; set; }
        public bool Handled { get; set; }
    }
    public delegate void ThrowErrorEventhandler(object sender, ThrowErrorEventArgs e);
    public static class ThrowErrorManerger
    {
        public static event ThrowErrorEventhandler ThrowError;
        public static bool HandleError = false;
    }
    public class ScriptException : Exception
    {
        public ScriptException(string message,Exceptions erorcode,ParsingScript script=null,ParsingException exception=null) : base(message)
        {
            this.ErrorCode= erorcode;
            this.Script = script;
            this.Exception=exception;
        }
        public ParsingScript Script { get; set; }
        public ParsingException Exception { get; set; }
        public Exceptions ErrorCode { get; set; }
        public bool Handled { get; set; }

        
    }
}
