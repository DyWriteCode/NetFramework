using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Net.Rpc
{
    public class RunRpcAttribute : Attribute
    {
        public string MethodName { get; }

        public RunRpcAttribute(string name = null)
        {
            MethodName = name;
        }
    }
}
