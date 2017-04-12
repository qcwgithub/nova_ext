using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    /// <summary>
    /// 协程管理器
    /// </summary>
    public class CoroutineManager
    {
        // 终止一个协程
        public void Stop(ICoroutine c)
        {
            coroutineList.Remove((Coroutine)c);
        }

        // 启动一个协程
        public ICoroutine Start(IEnumerator e)
        {
            return StartCoroutineInternal(e, false);
        }

        // 启动一个协程，第二参数表示是否立刻执行，false 表示下一帧开始执行
        public ICoroutine StartCoroutineInternal(IEnumerator e, bool runImmediately)
        {
            Coroutine c = new Coroutine(e);
            coroutineList.Add(c);

            if (runImmediately)
                c.Next(0);

            return c;
        }

        // 推动所有协程
        public void OnTimeElapsed(int te)
        {
            removed.Clear();

            // 推动每个协程
            Coroutine[] list = coroutineList.ToArray();
            foreach (Coroutine c in list)
            {
                if (removed.Contains(c))
                    continue;

                if (!c.Finished)
                    c.Next(te);

                if (c.Finished)
                    removed.Add(c);
            }

            // 把该移除的移除
            foreach (Coroutine c in removed)
                coroutineList.Remove(c);
        }

        // 所有协程
        List<Coroutine> coroutineList = new List<Coroutine>();
        List<Coroutine> removed = new List<Coroutine>();
    }
}
