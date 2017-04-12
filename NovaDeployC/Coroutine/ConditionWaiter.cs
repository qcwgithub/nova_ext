using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    // 在协程中等待一个条件结束
    public class ConditionWaiter
    {
        // 构造器，需要给出检查条件
        public ConditionWaiter(Func<bool> handler)
        {
            cch = handler;
        }

        // 条件是否结束
        public bool Finished
        {
            get
            {
                return cch();
            }
        }

        // 条件检查
        Func<bool> cch = null;
    }
}
