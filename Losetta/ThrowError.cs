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

        /// <summary>
        /// スクリプトの実行時に生じた例外を、ThrowErrorManergerでキャッチせずそのままスローする場合はTrue、それ以外の場合はFalse。
        /// </summary>
        public static bool NotCatch { get; set; }
        public static void OnThrowError(object sender,ThrowErrorEventArgs e)
        {
            ThrowError?.Invoke(sender, e);
        }
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
