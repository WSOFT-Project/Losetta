using AliceScript.Parsing;
using System.Text;

namespace AliceScript.Functions
{
    internal sealed class LiteralFunction : FunctionBase
    {
        public LiteralFunction()
        {
            Name = "Literal";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += literalFunction_Run;
        }

        private void literalFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            // 文字列型かどうか確認
            if (!string.IsNullOrEmpty(Item))
            {
                if (StringMode)
                {
                    bool sq = Item[0] == Constants.QUOTE1 && Item[^1] == Constants.QUOTE1;
                    bool dq = Item[0] == Constants.QUOTE && Item[^1] == Constants.QUOTE;
                    Name = "StringLiteral";
                    if (dq || sq)
                    {
                        //文字列型
                        string result = Item.Substring(1, Item.Length - 2);
                        //文字列補間
                        result = result.Replace(Constants.QUOTE_IN_LITERAL, Constants.QUOTE);
                        result = result.Replace(Constants.QUOTE1_IN_LITERAL, Constants.QUOTE1);

                        if (DetectionStringFormat)
                        {
                            var stb = new StringBuilder();
                            int blackCount = 0;
                            bool beforeEscape = false;
                            var nowBlack = new StringBuilder();


                            Name = "StringInterpolationLiteral";

                            foreach (char r in result)
                            {
                                switch (r)
                                {
                                    case Constants.START_GROUP:
                                        {
                                            if (blackCount == 0)
                                            {
                                                if (!beforeEscape)
                                                {
                                                    blackCount++;
                                                }
                                                else
                                                {
                                                    stb.Append(r);
                                                }
                                            }
                                            else
                                            {
                                                nowBlack.Append(r);
                                                blackCount++;
                                            }
                                            beforeEscape = false;
                                            break;
                                        }
                                    case Constants.END_GROUP:
                                        {
                                            if (blackCount == 1)
                                            {
                                                blackCount--;
                                                //この波かっこを抜けるとき
                                                string code = nowBlack.ToString();
                                                ParsingScript tempScript = e.Script.GetTempScript(code);
                                                var rrr = tempScript.Process();
                                                rrr ??= Variable.EmptyInstance;
                                                stb.Append(rrr.AsString());
                                                nowBlack.Clear();
                                            }
                                            else
                                            {
                                                if (!beforeEscape)
                                                {
                                                    blackCount--;
                                                    nowBlack.Append(r);
                                                }
                                                else
                                                {
                                                    stb.Append(r);
                                                }
                                            }
                                            beforeEscape = false;
                                            break;
                                        }
                                    case '\\':
                                        {
                                            beforeEscape = !beforeEscape;
                                            break;
                                        }
                                    default:
                                        {
                                            beforeEscape = false;
                                            if (blackCount > 0)
                                            {
                                                nowBlack.Append(r);
                                            }
                                            else
                                            {
                                                stb.Append(r);
                                            }
                                            break;
                                        }
                                }
                            }
                            if (blackCount > 0)
                            {
                                throw new ScriptException("波括弧が不足しています", Exceptions.NEED_BRACKETS, e.Script);
                            }
                            else if (blackCount < 0)
                            {
                                throw new ScriptException("終端の波括弧は不要です", Exceptions.UNNEED_TO_BRACKETS, e.Script);
                            }
                            result = stb.ToString();
                        }

                        if (DetectionUTF8_Literal)
                        {
                            //UTF-8リテラルの時はUTF-8バイナリを返す
                            e.Return = new Variable(Encoding.UTF8.GetBytes(result));
                            return;
                        }
                        else
                        {
                            e.Return = new Variable(result);
                            return;
                        }
                    }
                }
                else
                {
                    // 数値として処理
                    Name = "NumberLiteral";
                    double num = Utils.ConvertToDouble(Item, e.Script);
                    e.Return = new Variable(num);
                }
            }

        }

        public bool StringMode { get; set; }
        public string Item { private get; set; }
        public bool DetectionUTF8_Literal { get; set; }
        public bool DetectionStringFormat { get; set; }


    }
}
