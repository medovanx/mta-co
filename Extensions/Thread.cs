namespace MTA.ServerBase
{
    using MTA;
    using System;
    using System.Runtime.CompilerServices;
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
                Action execute = this.Executes;
                do
                {
                    action2 = execute;
                    Action action3 = (Action)Delegate.Combine(action2, value);
                    execute = Interlocked.CompareExchange<Action>(ref this.Executes, action3, action2);
                }
                while (execute != action2);
            }
            remove
            {
                Action action2;
                Action execute = this.Executes;
                do
                {
                    action2 = execute;
                    Action action3 = (Action)Delegate.Remove(action2, value);
                    execute = Interlocked.CompareExchange<Action>(ref this.Executes, action3, action2);
                }
                while (execute != action2);
            }
        }

        public Thread(int milliseconds)
        {
            this.Closed = false;
            this.Milliseconds = milliseconds;
        }

        private void Loop()
        {
            this.Sleep(0x5dc);
            while (true)
            {
                if (this.Closed)
                {
                    return;
                }
                try
                {
                    if (this.Executes != null)
                    {
                        this.Executes();
                    }
                }
                catch (Exception exception)
                {
                    Program.SaveException(exception);
                    MTA.Console.WriteLine(exception);
                }
                this.Sleep(this.Milliseconds);
            }
        }

        public void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public void Start()
        {
            this.base_thread = new System.Threading.Thread(new ThreadStart(this.Loop));
            this.base_thread.Start();
        }

        public bool Closed { get; set; }
    }
}

