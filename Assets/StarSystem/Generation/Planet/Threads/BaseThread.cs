using System;
using System.Threading;

namespace Assets.StarSystem.Generation.Planet.Threads
{
    public abstract class BaseThread
    {
        public bool isRunning;
        public SystemController controller;
        private Thread thread;

        private void Run()
        {
            while (isRunning)
            {
                try
                {
                    Action();
                }
                catch (Exception ex)
                {
                    //pass all thread exceptions to main thread
                    lock (controller.threadExceptions)
                    {
                        controller.threadExceptions.Add(ex);
                    }
                }
            }
        }

        public BaseThread(SystemController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Action is called in a loop and inheriting threads must implement it
        /// </summary>
        protected virtual void Action()
        {

        }

        public virtual void Start()
        {
            thread = new Thread(() => Run())
            {
                IsBackground = true
            };
            isRunning = true;
            thread.Start();
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void StopAndWait()
        {
            Stop();
            thread.Join();
        }

        public void Restart()
        {
            StopAndWait();
            Start();
        }
    }
}
