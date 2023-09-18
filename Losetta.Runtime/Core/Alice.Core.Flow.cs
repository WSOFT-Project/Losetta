using AliceScript.Parsing;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static void Readonly(ParsingScript script, Variable item)
        {
            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript parsingScript = script.GetTempScript(body);

            bool beforeState = item.Readonly;//もともとの状態を覚えておく
            item.Readonly = true;

            parsingScript.ExecuteAll();

            item.Readonly = beforeState;//実行後に元に戻す
        }
        public static void Lock(ParsingScript script, Variable item)
        {
            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript parsingScript = script.GetTempScript(body);

            lock (item)
            {
                parsingScript.ExecuteAll();
            }
        }
    }
}
