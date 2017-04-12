using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.Coroutine
{
    public class YieldOp
    {
        public YieldOp(IEnumerator e)
        {
            this.e = e;
        }

        public object Current()
        {
            return e.Current;
        }

        public bool MoveNext()
        {
            return this.e.MoveNext();
        }

        IEnumerator e;
    }
}
