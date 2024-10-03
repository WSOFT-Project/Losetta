using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System.Collections.Generic;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Reflection
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(ReflectionFunctions));
        }
    }
    [AliceNameSpace(Name = "Alice.Reflection")]
    internal static class ReflectionFunctions
    {
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static IEnumerable<Variable> Properties(this Variable v)
        {
            return v.GetProperties();
        }
        public static Variable Reflect_GetVariable(ParsingScript script, string varName)
        {
            varName = Constants.ConvertName(varName);
            return ParserFunction.GetVariable(varName, script) is ValueFunction getVar ? getVar.Value : Variable.EmptyInstance;
        }
        public static Variable Reflect_Get_Member(ParsingScript script, string memberName)
        {
            memberName = Constants.ConvertName(memberName);
            string action = string.Empty;
            var member = new ParserFunction(script, memberName, '\0', ref action).m_impl;
            if (member is ValueFunction valueFunction)
            {
                return valueFunction.Value;
            }
            else if (member is FunctionBase fb)
            {
                return new Variable(new DelegateObject(fb));
            }
            throw new ScriptException($"識別子`{memberName}`は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, script);
        }
    }
}
