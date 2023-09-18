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
            space.Add(new ExceptionObject());
            NameSpaceManager.Add(space);


            //総合関数(コアプロパティ)
            Variable.AddProp(new PropertiesProp());
            Variable.AddProp(new TypProp());
            //統合関数(終わり)
            //複合関数(複数の型に対応する関数)
            Variable.AddProp(new LengthSizeProp(), Constants.LENGTH);
            Variable.AddProp(new LengthSizeProp(), Constants.SIZE);
            Variable.AddProp(new KeysFunc());
            //複合関数(終わり)
            //DELEGATE系(Delegate.csに本体あり)
            Variable.AddProp(new DelegateNameFunc());
            //DELEGATE系(終わり)
        }
    }
    internal sealed class KeysFunc : PropertyBase
    {
        public KeysFunc()
        {
            Name = Constants.KEYS;
            CanSet = false;
            HandleEvents = true;
            Type = Variable.VarType.MAP_NUM | Variable.VarType.MAP_STR;
            Getting += KeysFunc_Getting;

        }

        private void KeysFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetAllKeys());
        }

    }

    internal sealed class PropertiesProp : PropertyBase
    {
        public PropertiesProp()
        {
            Name = Constants.OBJECT_PROPERTIES;
            CanSet = false;
            HandleEvents = true;
            Getting += PropertiesProp_Getting;
        }

        private void PropertiesProp_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetProperties());
        }
    }

    internal sealed class TypProp : PropertyBase
    {
        public TypProp()
        {
            Name = Constants.OBJECT_TYPE;
            CanSet = false;
            HandleEvents = true;
            Getting += TypProp_Getting;
        }

        private void TypProp_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = Variable.AsType(e.Parent.Type);
        }

    }

    internal sealed class LengthSizeProp : PropertyBase
    {
        public LengthSizeProp()
        {
            CanSet = false;
            HandleEvents = true;
            Type = Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY;
            Getting += LengthFunc_Getting;
        }

        private void LengthFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetLength());
        }

    }


    internal sealed class DelegateNameFunc : PropertyBase
    {
        public DelegateNameFunc()
        {
            Name = "Name";
            CanSet = false;
            HandleEvents = true;
            Getting += DelegateNameFunc_Getting;
        }

        private void DelegateNameFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.Delegate.Name);
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
                            if (args != null && args[mn] != null)
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
