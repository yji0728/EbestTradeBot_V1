using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbestTradeBot.Core.EventArgs
{
    public class LoginEventArgs : System.EventArgs
    {
        public string Code { get; }
        public string Message { get; }

        public LoginEventArgs(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
