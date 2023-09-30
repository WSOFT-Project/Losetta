using AliceScript.Functions;
using AliceScript.Objects;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using static AliceScript.Binding.BindFunction;

namespace AliceScript.Binding
{
    internal class BindValueFunction : ValueFunction
    {
        public BindValueFunction()
        {
            Setting += BindValueFunction_Setting;
            Getting += BindValueFunction_Getting;
        }

        private void BindValueFunction_Getting(object sender, PropertyBaseEventArgs e)
        {
            if (Get != null)
            {
                e.Value = new Variable(Get.ObjFunc.Invoke(new object[] { }));
                return;
            }
            throw new ScriptException($"`{Name}`に対応するオーバーロードを解決できませんでした", Exceptions.COULDNT_FIND_FUNCTION);
        }

        private void BindValueFunction_Setting(object sender, PropertyBaseEventArgs e)
        {
            if (Set != null)
            {
                FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
                ex.Args = new List<Variable> { e.Value };

                if (Set.TryConvertParameters(ex, this, out var args))
                {
                    Set.VoidFunc.Invoke(args);
                    return;
                }

                throw new ScriptException($"`{Name}`に対応するオーバーロードを解決できませんでした", Exceptions.COULDNT_FIND_FUNCTION);
            }
        }

        public BindingOverloadFunction Set { get; set; }
        public BindingOverloadFunction Get { get; set; }
        public static BindValueFunction CreateBindValueFunction(PropertyInfo propertyInfo)
        {
            var func = new BindValueFunction();
            func.Name = propertyInfo.Name;
            func.HandleEvents = true;

            var getFunc = propertyInfo.GetGetMethod();
            var setFunc = propertyInfo.GetSetMethod();

            if (TryGetAttibutte<AlicePropertyAttribute>(propertyInfo, out var attribute))
            {
                if (attribute.Name != null)
                {
                    func.Name = attribute.Name;
                }
            }

            if (setFunc != null && setFunc.IsPublic)
            {
                func.CanSet = true;
                var load = new BindingOverloadFunction();
                load.TrueParameters = setFunc.GetParameters();
                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();
                load.VoidFunc = Expression.Lambda<Action<object[]>>(
                Expression.Convert(
                    Expression.Call(setFunc, parameters),
                    typeof(void)),
                args).Compile();
                load.IsVoidFunc = true;

                func.Set = load;
            }
            if (getFunc != null && getFunc.IsPublic)
            {
                var load = new BindingOverloadFunction();
                load.TrueParameters = getFunc.GetParameters();
                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();

                load.ObjFunc = Expression.Lambda<Func<object[], object>>(
                    Expression.Convert(
                        Expression.Call(getFunc, parameters),
                        typeof(object)),
                    args).Compile();

                func.Get = load;
            }
            return func;
        }

    }
}
