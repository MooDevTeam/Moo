﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Moo.Core.Daemon
{
    public abstract class Daemon
    {
        Thread thread;
        volatile bool running;

        protected abstract int Run();

        public void Start()
        {
            if (thread != null)
            {
                Stop();
            }

            running = true;
            thread = new Thread(ThreadMain);
            thread.Start();
        }

        public void Stop()
        {
            running = false;
            thread.Interrupt();
            thread.Join();
            thread = null;
        }

        public void ThreadMain()
        {
            while (running)
            {
                try
                {
                    Thread.Sleep(Run());
                }
                catch (Exception e)
                {
                    if (e is ThreadInterruptedException)
                    {

                    }
                    else
                    {
                        //Should Log?
                    }
                }
            }
        }
    }
}
