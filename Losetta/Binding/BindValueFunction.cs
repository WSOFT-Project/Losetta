﻿using AliceScript.Functions;
using System.Collections.Generic;

namespace AliceScript.Binding
{
    /// <summary>
    /// .NETのプロパティと対応するAliceScriptの関数
    /// </summary>
    public class BindValueFunction : ValueFunction
    {
        public BindValueFunction()
        {
            Setting += BindValueFunction_Setting;
            Getting += BindValueFunction_Getting;
        }

        private void BindValueFunction_Getting(object sender, ValueFunctionEventArgs e)
        {
            if (Get is not null)
            {
                e.Value = Get.IsInstanceFunc
                    ? new Variable(Get.InstanceObjFunc.Invoke(Parent?.Instance, new object[] { }))
                    : new Variable(Get.ObjFunc.Invoke(new object[] { }));
                return;
            }
            throw new ScriptException($"`{Name}`に対応するオーバーロードを解決できませんでした", Exceptions.COULDNT_FIND_FUNCTION);
        }

        private void BindValueFunction_Setting(object sender, ValueFunctionEventArgs e)
        {
            if (Set is not null)
            {
                FunctionBaseEventArgs ex = new FunctionBaseEventArgs();
                ex.Args = new List<Variable> { e.Value };

                if (Set.TryConvertParameters(ex, this, out var args))
                {
                    if (Set.IsInstanceFunc)
                    {
                        Set.InstanceVoidFunc.Invoke(Parent?.Instance, args);
                    }
                    else
                    {
                        Set.VoidFunc.Invoke(args);
                    }
                    return;
                }

                throw new ScriptException($"`{Name}`に対応するオーバーロードを解決できませんでした", Exceptions.COULDNT_FIND_FUNCTION);
            }
        }

        public BindingOverloadFunction Set { get; set; }
        public BindingOverloadFunction Get { get; set; }
        public BindObject Parent { get; set; }
    }
}
