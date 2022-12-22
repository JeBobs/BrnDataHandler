using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrnDataHandler
{
    abstract class Command
    {
        public abstract bool Run();
    }
}
