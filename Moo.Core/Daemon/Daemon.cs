using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Moo.Core.Utility;
namespace Moo.Core.Daemon
{
    public abstract class Daemon
    {
        public static void StartAll()
        {
            ContestDaemon.Instance.Start();
            TestDaemon.Instance.Start();
            FullIndexDaemon.Instance.Start();
        }

        public static void StopAll()
        {
            ContestDaemon.Instance.Stop();
            TestDaemon.Instance.Stop();
            FullIndexDaemon.Instance.Stop();
        }

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
                        //Going to Stop
                    }
                    else
                    {
                        using (var log = Config.LogFile.AppendText())
                        {
                            log.WriteLine(new DateTime());
                            log.WriteLine(e);
                        }
                        //Sleep a while to prevent fill the disk
                        Thread.Sleep(10000);
                    }
                }
            }
        }
    }
}
