using AliceScript.Parsing;
using System;

namespace AliceScript
{
    /// <summary>
    /// スクリプトを実行中に発生した例外
    /// </summary>
    public class ThrowErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 例外の状態を表すメッセージ
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 例外について説明するサイトへのリンク
        /// </summary>
        public string HelpLink { get; set; }
        /// <summary>
        /// 例外が発生した場所
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 例外が発生したスクリプト
        /// </summary>
        public ParsingScript Script { get; set; }
        /// <summary>
        /// パース中の例外のとき、パーサーからの報告内容
        /// </summary>
        public ParsingException Exception { get; set; }
        /// <summary>
        /// 例外を表すエラーコード
        /// </summary>
        public Exceptions ErrorCode { get; set; }
        /// <summary>
        /// 例外が処理され場合はtrue
        /// </summary>
        public bool Handled { get; set; }
    }
    public delegate void ThrowErrorEventhandler(object sender, ThrowErrorEventArgs e);
    public static class ThrowErrorManager
    {
        public static event ThrowErrorEventhandler ThrowError;

        /// <summary>
        /// スクリプトの実行時に生じた例外を、ThrowErrorManagerでキャッチせずそのままスローする場合はTrue、それ以外の場合はFalse。
        /// </summary>
        public static bool NotCatch { get; set; }
        public static void OnThrowError(object sender, ThrowErrorEventArgs e)
        {
            ThrowError?.Invoke(sender, e);
            if (!e.Handled)
            {
                Alice.OnExiting(256);
            }
        }
    }
    public class ScriptException : Exception
    {
        public ScriptException(string message, Exceptions erorcode, ParsingScript script = null, ParsingException exception = null) : base(message)
        {
            ErrorCode = erorcode;
            Script = script;
            Exception = exception;
            HelpLink = Constants.HELP_LINK + ((int)ErrorCode).ToString("x3");
        }
        public ParsingScript Script { get; set; }
        public ParsingException Exception { get; set; }
        public Exceptions ErrorCode { get; set; }
        public bool Handled { get; set; }
    }
}
