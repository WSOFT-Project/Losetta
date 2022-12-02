using System;
using System.Collections.Generic;
using System.Text;

namespace AliceScript
{
    public class ThrowErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public ParsingScript Script { get; set; }
        public ParsingException Exception { get; set; }
        public Exceptions ErrorCode { get; set; }
    }
    public delegate void ThrowErrorEventhandler(object sender, ThrowErrorEventArgs e);
    public static class ThrowErrorManerger
    {
        public static event ThrowErrorEventhandler ThrowError;
        public static bool HandleError = false;
        public static bool InTryBlock = false;
        public static void OnThrowError(string message,Exceptions errorcode,  ParsingScript script = null,ParsingException exception=null,bool isHandled=false)
        {
            if (!InTryBlock)
            {
                ThrowErrorEventArgs ex = new ThrowErrorEventArgs();
                ex.Message = message;
                ex.ErrorCode = errorcode;
                ex.Exception = exception;
                if (script != null)
                {
                    ex.Script = script;
                    if (script.InTryBlock) { return; }
                }

                ThrowError?.Invoke(null, ex);

                if (isHandled)
                {
                    throw new HandledErrorException();
                }
            }
        }
    }
    public class HandledErrorException : Exception
    {

    }
}
