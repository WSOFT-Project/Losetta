using AliceScript.Functions;
using AliceScript.Parsing;

namespace AliceScript.Objects
{
    public class ExceptionObject : ObjectBase
    {
        public string Message { get; set; }
        public string HelpLink { get; set; }
        public string Source { get; set; }
        public Exceptions ErrorCode { get; set; }
        public ParsingScript MainScript { get; set; }
        public ExceptionObject(string message, Exceptions errorcode, ParsingScript mainScript, string source = null, string helplink = null)
        {
            Name = "Exception";
            Message = message;
            ErrorCode = errorcode;
            MainScript = mainScript;
            Source = source;
            HelpLink = helplink;
            Constructor = new Exception_Constractor();
            AddProperty(new Exception_MessageProperty(this));
            AddProperty(new Exception_SourceProperty(this));
            AddProperty(new Exception_HelpLinkProperty(this));
            AddProperty(new Exception_ErrorcodeProperty(this));
            AddProperty(new Exception_StackTraceProperty(this));
            AddFunction(new Exception_ToStringFunc(this));
        }
        public ExceptionObject()
        {
            Name = "Exception";
            Constructor = new Exception_Constractor();
        }
        public override string ToString()
        {
            return ErrorCode.ToString() + "(0x" + ((int)ErrorCode).ToString("x3") + ")" + (string.IsNullOrWhiteSpace(Message) ? string.Empty : ": " + Message);
        }

        private class Exception_Constractor : FunctionBase
        {
            public Exception_Constractor()
            {
                Run += Exception_Constractor_Run;
            }

            private void Exception_Constractor_Run(object sender, FunctionBaseEventArgs e)
            {
                switch (e.Args.Count)
                {
                    case 0:
                        {
                            var exc = new ExceptionObject(string.Empty, Exceptions.USER_DEFINED, e.Script);
                            e.Return = new Variable(exc);
                            break;
                        }
                    case 1:
                        {
                            if (e.Args[0].Type.HasFlag(Variable.VarType.NUMBER))
                            {
                                var exc = new ExceptionObject(string.Empty, (Exceptions)e.Args[0].AsInt(), e.Script);
                                e.Return = new Variable(exc);
                            }
                            else
                            {
                                var exc = new ExceptionObject(e.Args[0].AsString(), Exceptions.USER_DEFINED, e.Script);
                                e.Return = new Variable(exc);
                            }
                            break;
                        }
                    case 2:
                        {
                            var exc = new ExceptionObject(e.Args[1].AsString(), (Exceptions)e.Args[0].AsInt(), e.Script);
                            e.Return = new Variable(exc);
                            break;
                        }
                    case 3:
                        {
                            var exc = new ExceptionObject(e.Args[1].AsString(), (Exceptions)e.Args[0].AsInt(), e.Script, e.Args[2].AsString());
                            e.Return = new Variable(exc);
                            break;
                        }
                    case 4:
                        {
                            var exc = new ExceptionObject(e.Args[1].AsString(), (Exceptions)e.Args[0].AsInt(), e.Script, e.Args[2].AsString(), e.Args[3].AsString());
                            e.Return = new Variable(exc);
                            break;
                        }
                }
            }
        }
        private class Exception_MessageProperty : PropertyBase
        {
            public Exception_MessageProperty(ExceptionObject eo)
            {
                Name = "Message";
                HandleEvents = true;
                CanSet = false;
                ExceptionObject = eo;
                Getting += Exception_MessageProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_MessageProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(ExceptionObject.Message);
            }
        }
        private class Exception_SourceProperty : PropertyBase
        {
            public Exception_SourceProperty(ExceptionObject eo)
            {
                Name = "Source";
                HandleEvents = true;
                CanSet = false;
                ExceptionObject = eo;
                Getting += Exception_SourceProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_SourceProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(ExceptionObject.Source);
            }
        }
        private class Exception_HelpLinkProperty : PropertyBase
        {
            public Exception_HelpLinkProperty(ExceptionObject eo)
            {
                Name = "HelpLink";
                HandleEvents = true;
                CanSet = false;
                ExceptionObject = eo;
                Getting += Exception_HelpLinkProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_HelpLinkProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(ExceptionObject.HelpLink);
            }
        }

        private class Exception_StackTraceProperty : PropertyBase
        {
            public Exception_StackTraceProperty(ExceptionObject eo)
            {
                Name = "StackTrace";
                HandleEvents = true;
                CanSet = false;
                ExceptionObject = eo;
                Getting += Exception_StackTraceProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_StackTraceProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = ExceptionObject.MainScript.GetStackTrace();
            }
        }

        private class Exception_ErrorcodeProperty : PropertyBase
        {
            public Exception_ErrorcodeProperty(ExceptionObject eo)
            {
                Name = "ErrorCode";
                HandleEvents = true;
                CanSet = false;
                ExceptionObject = eo;
                Getting += Exception_MessageProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_MessageProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable((int)ExceptionObject.ErrorCode);
            }
        }

        private class Exception_ToStringFunc : FunctionBase
        {
            public Exception_ToStringFunc(ExceptionObject eo)
            {
                Name = "tostring";
                ExceptionObject = eo;
                Run += Exception_ToStringFunc_Run;
            }

            private void Exception_ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(ExceptionObject.ToString());
            }

            public ExceptionObject ExceptionObject { get; set; }
        }
    }
}
