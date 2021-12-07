using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Bobii.EventArg
{
    public class WriteConsoleEventArg : EventArgs
    {
        public string Message { get; set; }
        public bool Error { get; set; }
    }
}
