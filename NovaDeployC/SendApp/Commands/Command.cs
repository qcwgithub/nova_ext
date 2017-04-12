using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.SendApp.Commands
{
    public class Command
    {
        public virtual void Recv() { }
        public virtual IEnumerator Send() { yield break; }
    }
}
