using AliceScript.Functions;

namespace AliceScript
{
    public interface ICallingHandleAttribute
    {
        public void PreCall(FunctionBase function, FunctionBaseEventArgs args);
        public void PostCall(FunctionBase function, FunctionBaseEventArgs args);
    }
}
