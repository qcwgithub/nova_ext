using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    // 等待另外一个协程结束
    public class CoroutineWaiter : ConditionWaiter
    {
        // 构造器，指明要等待的协程
        public CoroutineWaiter(ICoroutine coroutine)
            : base(() => { return coroutine.Finished; })
        {
        }
    }
}
