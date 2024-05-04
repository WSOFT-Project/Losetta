﻿using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    internal class LibImportFunction : AttributeFunction
    {
        public LibImportFunction()
        {
            Name = Constants.USER_CANT_USE_FUNCTION_PREFIX + Constants.LIBRARY_IMPORT;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            LibraryName = Utils.GetSafeString(e.Args, 0);
            EntryPoint = Utils.GetSafeString(e.Args, 1, null);
            if (e.Args.Count > 3)
            {
                UseUnicode = e.Args[2].m_bool;
            }
        }
        public string LibraryName { get; set; }
        public string EntryPoint { get; set; }
        public bool? UseUnicode { get; set; }
    }
}