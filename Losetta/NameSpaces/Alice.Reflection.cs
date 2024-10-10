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
        public static IEnumerable<Variable> GetProperties(Variable v)
        {
            return v.GetProperties();
        }
        public static Variable Reflect_Get_Variable(ParsingScript script, string identifier)
        {
            var member = GetMember(script, identifier);
            if(member is ValueFunction valueFunction)
            {
                return valueFunction.Value;
            }
            throw new ScriptException($"`{identifier}`は現在のコンテキストに存在しません。", Exceptions.IDENTIFIER_NOT_FOUND, script);
        }
        public static Variable Reflect_Get_Function(ParsingScript script, string identifier)
        {
            var member = GetMember(script, identifier);
            if (member is FunctionBase func)
            {
                return new Variable(new DelegateObject(func));
            }
            throw new ScriptException($"`{identifier}`は現在のコンテキストに存在しません。", Exceptions.IDENTIFIER_NOT_FOUND, script);
        }
        public static Variable Reflect_Get_Member(ParsingScript script, string identifier)
        {
            var member = GetMember(script, identifier);
            if (member is ValueFunction valueFunction)
            {
                return valueFunction.Value;
            }
            else if (member is FunctionBase fb)
            {
                return new Variable(new DelegateObject(fb));
            }
            throw new ScriptException($"`{identifier}`は現在のコンテキストに存在しません。", Exceptions.IDENTIFIER_NOT_FOUND, script);
        }
        private static ParserFunction GetMember(ParsingScript script, string identifier)
        {
            identifier = Constants.ConvertName(identifier);
            string action = string.Empty;
            var member = new ParserFunction(script, identifier, '\0', ref action)?.m_impl;
            if(member is not null)
            {
                return member;
            }
            throw new ScriptException($"`{identifier}`は現在のコンテキストに存在しません。", Exceptions.IDENTIFIER_NOT_FOUND, script);
        }
    }
}
