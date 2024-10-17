using AliceScript.Functions;
using System;
using System.Collections.Generic;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Legacy
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Legacy");

            space.Add(new GetColumnFunction());
            space.Add(new SetPropertyFunction());
            space.Add(new GetPropertyFunction());
            space.Add(new GetPropertiesFunction());
            space.Add(new GetAllKeysFunction());

            NameSpaceManager.Add(space);
        }
    }
    internal sealed class GetPropertiesFunction : FunctionBase, IArrayFunction
    {
        public GetPropertiesFunction()
        {
            Name = "GetProperties";
            MinimumArgCounts = 1;
            Run += GetPropertiesFunction_Run;
        }

        private void GetPropertiesFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable baseValue = e.Args[0];
            List<Variable> props = baseValue.GetProperties();
            e.Return = new Variable(props);
        }
    }

    internal sealed class GetPropertyFunction : FunctionBase, IArrayFunction
    {
        public GetPropertyFunction()
        {
            Name = Constants.GET_PROPERTY;
            MinimumArgCounts = 2;
            Run += GetPropertyFunction_Run;
        }

        private void GetPropertyFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable baseValue = e.Args[0];
            string propName = Utils.GetSafeString(e.Args, 1);

            Variable propValue = baseValue.GetProperty(propName, e.Script);
            Utils.CheckNotNull(propValue, propName, e.Script);

            e.Return = new Variable(propValue);
        }
    }

    internal sealed class SetPropertyFunction : FunctionBase, IArrayFunction
    {
        public SetPropertyFunction()
        {
            Name = "SetProperty";
            MinimumArgCounts = 3;
            Run += SetPropertyFunction_Run;
        }

        private void SetPropertyFunction_Run(object sender, FunctionBaseEventArgs e)
        {

            Variable baseValue = e.Args[0];
            string propName = Utils.GetSafeString(e.Args, 1);
            Variable propValue = Utils.GetSafeVariable(e.Args, 2);

            Variable result = baseValue.SetProperty(propName, propValue, e.Script);

            AddGlobalOrLocalVariable(baseValue.ParsingToken,
                                                    new ValueFunction(baseValue), e.Script);
            e.Return = result;
        }
    }

    internal sealed class GetColumnFunction : FunctionBase, IArrayFunction
    {
        public GetColumnFunction()
        {
            Name = "GetColumn";
            MinimumArgCounts = 2;
            Run += GetColumnFunction_Run;
        }

        private void GetColumnFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable arrayVar = Utils.GetSafeVariable(e.Args, 0);
            int col = Utils.GetSafeInt(e.Args, 1);
            int fromCol = Utils.GetSafeInt(e.Args, 2, 0);

            var tuple = arrayVar.Tuple;

            List<Variable> result = new List<Variable>(tuple.Count);
            for (int i = fromCol; i < tuple.Count; i++)
            {
                Variable current = tuple[i];
                if (current.Tuple is null || current.Tuple.Count <= col)
                {
                    throw new ArgumentException(m_name + ": Index [" + col + "] doesn't exist in column " +
                                                i + "/" + (tuple.Count - 1));
                }
                result.Add(current.Tuple[col]);
            }

            e.Return = new Variable(result);
        }

    }

    internal sealed class GetAllKeysFunction : FunctionBase, IArrayFunction
    {
        public GetAllKeysFunction()
        {
            Name = "GetAllKeys";
            MinimumArgCounts = 1;
            Run += GetAllKeysFunction_Run;
        }

        private void GetAllKeysFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            List<Variable> results = e.Args[0].GetAllKeys();

            e.Return = new Variable(results);
        }
    }
}
