using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    // 协程事件等待
    public class EventWaiter
    {
        // 构造器，默认手动重置，不超时
        public EventWaiter()
            : this(false, 0)
        {
        }

        // 构造器，默认不超时
        public EventWaiter(bool autoReset)
            : this(autoReset, 0)
        {
        }

        // 构造器，指明是自动还是手动重置和超时时间（<=0表示永远不超时）
        public EventWaiter(bool autoReset, int timeout)
        {
            arst = autoReset;
            interval = timeout;
            t = interval;
        }

        // 是否已经超时
        public bool Expired
        {
            get
            {
                return expired;
            }
        }

        // 处理时间流逝
        public void TimeElapsed(int te)
        {
            if (interval <= 0 || expired)
                return;

            t -= te;
            expired = (t <= 0);
        }

        // 是否自动重置
        public bool IsAutoReset
        {
            get
            {
                return arst;
            }
        }

        // 是否已经触发
        public bool IsSet
        {
            get
            {
                return set;
            }
        }

        // 触发事件
        public void Set()
        {
            set = true;
        }

        // 重制事件
        public void Reset()
        {
            set = false;
            t = interval;
        }

        // 是否已经触发
        bool set = false;

        // 是否是自动重置
        bool arst = false;

        // 是否已经超时
        bool expired = false;

        // 超时间隔时间
        int interval = 0;

        // 超时倒计时
        int t = 0;
    }
}
