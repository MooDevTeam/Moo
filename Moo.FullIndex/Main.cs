using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moo.FullIndex
{
    public class Main : Moo.Core.Daemon.Daemon
    {
        public static Main Instance = new Main();
        protected override int Run()
        {
            Console.Beep();
            return 0;
        }
    }
}
