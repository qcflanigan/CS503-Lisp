using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Lisp{

    public class Return : Exception{
        public object value;

        public Return(object value){
            this.value = value;
        }
    }
    public class EmptyReturn : Exception{
        
    }
}