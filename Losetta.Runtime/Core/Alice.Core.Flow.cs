using AliceScript.Binding;
using AliceScript.Parsing;
using System.Collections;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        /// <summary>
        /// このブロック内で、指定された変数をすべて読み取り専用にします
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="items">読み取り専用にしたい変数</param>
        public static void Readonly(ParsingScript script, params Variable[] items)
        {
            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript parsingScript = script.GetTempScript(body);

            BitArray beforeStates = new BitArray(items.Length);

            for (int i = 0; i < items.Length; i++)//もともとの状態を覚えておく
            {
                beforeStates[i] = items[i].Readonly;
                items[i].Readonly = true;
            }

            parsingScript.ExecuteAll();

            for (int i = 0; i < items.Length; i++)//実行後に元に戻す
            {
                items[i].Readonly = beforeStates[i];
            }
        }
        /// <summary>
        /// このブロック内で、指定された変数への排他的なアクセスを保証します
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="item">排他的ロックを行いたい変数</param>
        public static void Lock(ParsingScript script, Variable item)
        {
            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript parsingScript = script.GetTempScript(body);

            lock (item)
            {
                parsingScript.ExecuteAll();
            }
        }
        /// <summary>
        /// 指定された式が真と評価されたときに、本文を実行します
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="func">この関数がバインドされるFunctionBase</param>
        /// <param name="condition">本文を実行するかどうかを決める条件</param>
        /// <returns></returns>
        public static Variable If(ParsingScript script, BindFunction func, bool? condition)
        {
            Variable result = Variable.EmptyInstance;
            bool? isTrue = condition;

            if (isTrue == true)
            {
                result = script.ProcessBlock();

                if (result != null && (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE))
                {
                    // if文中で早期リターンしたからブロックごと飛ばす
                    script.SkipBlock();
                }
                script.SkipRestBlocks();

                return result != null && (result.IsReturn ||
                       result.Type == Variable.VarType.BREAK ||
                       result.Type == Variable.VarType.CONTINUE) ? result : Variable.EmptyInstance;
            }

            // elseブロックがあったら飛ばす
            script.SkipBlock();

            ParsingScript nextData = new ParsingScript(script);
            nextData.ParentScript = script;

            string nextToken = Utils.GetNextToken(nextData);

            if (Constants.ELSE_IF == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = func.Execute(script);
            }
            else if (Constants.ELSE == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;

                // 一応その次のトークンも調べる
                nextToken = Utils.GetNextToken(nextData);

                if (Constants.IF == nextToken)
                {
                    // もしelseの次がifなら、else ifのため続きで実行
                    script.Pointer = nextData.Pointer + 1;
                    result = func.Execute(script);
                }
                else
                {
                    result = script.ProcessBlock();
                }

            }
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }

            return result.IsReturn ||
                   result.Type == Variable.VarType.BREAK ||
                   result.Type == Variable.VarType.CONTINUE ? result : Variable.EmptyInstance;
        }
    }
}
