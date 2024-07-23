using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Helper
{
    public class MathHelper
    {
        /// <summary>
        /// 比较两个数字是否相等
        /// </summary>
        /// <param name="a">第一个数字</param>
        /// <param name="b">第二个数字</param>
        /// <returns></returns>
        public static bool Equals(float a, float b)
        {
            return MathF.Abs(a - b) < 10e-6; //0.00001
        }
    }
}
