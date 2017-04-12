using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    /// <summary>
    /// 协程
    /// </summary>
    class Coroutine : ICoroutine
    {
        // 构造器，需要提供对应的迭代器
        public Coroutine(IEnumerator enumerator)
        {
            //e = enumerator;
            op = new YieldOp(enumerator);
        }

        // 推动协程
        public void Next(int te)
        {
            this.C = op.Current();

            if ((C is TimeWaiter) && ((TimeWaiter)C).t > 0)   // 等待时间
                ((TimeWaiter)C).t -= te;
            else if ((C is FrameWaiter) && ((FrameWaiter)C).f > 0)    // 等待帧数
                ((FrameWaiter)C).f--;
            else if ((C is ConditionWaiter) && !((ConditionWaiter)C).Finished)  // 条件等待
                return;
            else if (C is EventWaiter)  // 事件等待
            {
                EventWaiter ew = (EventWaiter)C;
                if (!ew.IsSet && !ew.Expired)
                {
                    ew.TimeElapsed(te);
                    return;
                }
                else
                {
                    finished = !op.MoveNext();
                    if (ew.IsAutoReset)
                        ew.Reset();
                }
            }
            else if ((C is Coroutine) && !((Coroutine)C).Finished) // 如果还有子协程，则先等子协程处理完
                return;
            else
                finished = !op.MoveNext();
        }

        // 协程是否已经结束
        public bool Finished
        {
            get
            {
                return finished;
            }
            set
            {
                finished = value;
            }
        }

        // 要执行的迭代器
        //IEnumerator e = null;
        YieldOp op = null;
        // 结束标记
        bool finished = false;

        object C = null; // e.Current
    }
}
