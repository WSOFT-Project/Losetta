using AliceScript.Functions;
using AliceScript.Objects;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    //このクラスはデフォルトで読み込まれるため読み込み処理が必要です
    internal sealed class Alice_Initer
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(Core.CoreFunctions));
            NameSpace space = new NameSpace(Constants.TOP_NAMESPACE);
            space.Add<ExceptionObject>();
            NameSpaceManager.Add(space);
        }
    }
    internal sealed class StringFormatFunction : FunctionBase
    {
        public static string Format(string format, Variable[] args)
        {
            string text = format;
            MatchCollection mc = Regex.Matches(format, @"{[0-9]+:?[a-z,A-Z]*}");
            foreach (Match match in mc)
            {
                int mn = -1;
                string indstr = match.Value.TrimStart('{').TrimEnd('}');
                bool selectSubFormat = false;
                string subFormat = "";
                if (indstr.Contains(":"))
                {
                    string[] vs = indstr.Split(':');
                    indstr = vs[0];
                    if (!string.IsNullOrEmpty(vs[1]))
                    {
                        selectSubFormat = true;
                        subFormat = vs[1];
                    }
                }
                if (int.TryParse(indstr, out mn))
                {
                    if (args.Length > mn)
                    {
                        if (selectSubFormat)
                        {
                            switch (args[mn].Type)
                            {
                                case Variable.VarType.NUMBER:
                                    {
                                        switch (subFormat.ToLowerInvariant())
                                        {
                                            case "c":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("c"));
                                                    break;
                                                }
                                            case "d":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("d"));
                                                    break;
                                                }
                                            case "e":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("e"));
                                                    break;
                                                }
                                            case "f":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("f"));
                                                    break;
                                                }
                                            case "g":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("g"));
                                                    break;
                                                }
                                            case "n":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("n"));
                                                    break;
                                                }
                                            case "p":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("p"));
                                                    break;
                                                }
                                            case "r":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("r"));
                                                    break;
                                                }
                                            case "x":
                                                {
                                                    text = text.Replace(match.Value, ((int)args[mn].Value).ToString("x"));
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (args is not null && args[mn] is not null)
                            {
                                text = text.Replace(match.Value, args[mn].AsString());
                            }
                        }
                    }
                    else
                    {
                        //範囲外のためスキップ
                        continue;
                    }
                }
                else
                {
                    //数字ではないためスキップ
                    continue;
                }

            }
            return text;
        }
    }
}
