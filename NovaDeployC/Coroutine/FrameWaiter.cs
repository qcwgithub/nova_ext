using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    // 利用协程等待指定帧数
    public class FrameWaiter
    {
        // 构造器，指明要等待的帧数
        public FrameWaiter(int frame)
        {
            f = frame;
        }

        // 剩余帧数
        public int f = 0;
    }
}
