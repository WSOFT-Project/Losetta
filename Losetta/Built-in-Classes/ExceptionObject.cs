namespace AliceScript
{
    public class ExceptionObject : ObjectBase
    {
        public string Message { get; set; }
        public Exceptions ErrorCode { get; set; }
        public ParsingScript MainScript { get; set; }
        public ExceptionObject(string message, Exceptions errorcode, ParsingScript mainScript)
        {
            this.Message = message;
            this.ErrorCode = errorcode;
            this.MainScript = mainScript;
            this.AddProperty(new Exception_MessageProperty(this));
            this.AddProperty(new Exception_ErrorcodeProperty(this));
            this.AddProperty(new Exception_StackTraceProperty(this));
            this.AddFunction(new Exception_ToStringFunc(this));
        }
        public override string ToString()
        {
            return "[0x" + ((int)ErrorCode).ToString("x3") + "]" + ErrorCode.ToString() + " : " + Message;
        }

        private class Exception_MessageProperty : PropertyBase
        {
            public Exception_MessageProperty(ExceptionObject eo)
            {
                this.Name = "Message";
                this.HandleEvents = true;
                this.CanSet = false;
                this.ExceptionObject = eo;
                this.Getting += Exception_MessageProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_MessageProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(ExceptionObject.Message);
            }
        }

        private class Exception_StackTraceProperty : PropertyBase
        {
            public Exception_StackTraceProperty(ExceptionObject eo)
            {
                this.Name = "StackTrace";
                this.HandleEvents = true;
                this.CanSet = false;
                this.ExceptionObject = eo;
                this.Getting += Exception_StackTraceProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_StackTraceProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = ExceptionObject.MainScript.GetStackTrace();
            }
        }

        private class Exception_ErrorcodeProperty : PropertyBase
        {
            public Exception_ErrorcodeProperty(ExceptionObject eo)
            {
                this.Name = "ErrorCode";
                this.HandleEvents = true;
                this.CanSet = false;
                this.ExceptionObject = eo;
                this.Getting += Exception_MessageProperty_Getting;
            }
            public ExceptionObject ExceptionObject { get; set; }
            private void Exception_MessageProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable((int)ExceptionObject.ErrorCode);
            }
        }

        private class Exception_ToStringFunc : FunctionBase
        {
            public Exception_ToStringFunc(ExceptionObject eo)
            {
                this.Name = "tostring";
                this.ExceptionObject = eo;
                this.Run += Exception_ToStringFunc_Run;
            }

            private void Exception_ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = new Variable(ExceptionObject.ToString());
            }

            public ExceptionObject ExceptionObject { get; set; }
        }
    }
}
