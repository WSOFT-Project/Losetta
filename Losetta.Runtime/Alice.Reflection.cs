using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Parsing;

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
    }
}
