using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    /// <summary>
    /// 协程接口
    /// </summary>
    public interface ICoroutine
    {
        // 协程是否已经结束
        bool Finished
        {
            get;
        }
    }
}
