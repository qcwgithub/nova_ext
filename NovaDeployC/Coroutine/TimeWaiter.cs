using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    public class TimeWaiter
    {
        // 构造器，指明等待事件，单位 ms
        public TimeWaiter(int time)
        {
            t = time;
        }

        // 剩余等待时间
        public int t = 0;
    }
}
