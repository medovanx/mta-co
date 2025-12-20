namespace MTA.ServerBase
{
    using MTA;
    using System;
    using System.Threading;

    public class Thread
    {
        private System.Threading.Thread base_thread;
        private Action Executes;
        private int Milliseconds;

        public event Action Execute
        {
            add
            {
                Action action2;
                Action execute = Executes;
                do
                {
                    action2 = execute;
                    Action action3 = (Action)Delegate.Combine(action2, value);
                    execute = Interlocked.CompareExchange<Action>(ref Executes, action3, action2);
                }
                while (execute != action2);
            }
            remove
            {
                Action action2;
                Action execute = Executes;
                do
                {
                    action2 = execute;
                    Action action3 = (Action)Delegate.Remove(action2, value);
                    execute = Interlocked.CompareExchange<Action>(ref Executes, action3, action2);
                }
                while (execute != action2);
            }
        }

        public Thread(int milliseconds)
        {
            Closed = false;
            Milliseconds = milliseconds;
        }

        private void Loop()
        {
            Sleep(0x5dc);
            while (true)
            {
                if (Closed)
                {
                    return;
                }
                try
                {
                    if (Executes != null)
                    {
                        Executes();
                    }
                }
                catch (Exception exception)
                {
                    Program.SaveException(exception);
                    MTA.Console.WriteLine(exception);
                }
                Sleep(Milliseconds);
            }
        }

        public void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public void Start()
        {
            base_thread = new System.Threading.Thread(new ThreadStart(Loop));
            base_thread.Start();
        }

        public bool Closed { get; set; }
    }
}

