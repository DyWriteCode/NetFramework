using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Net.Rpc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RunRpcAttribute : Attribute
    {
        public string Name { get; }

        public RunRpcAttribute(string name = null)
        {
            Name = name;
        }
    }
}
