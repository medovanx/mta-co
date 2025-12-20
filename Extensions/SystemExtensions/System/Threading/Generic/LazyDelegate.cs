namespace System.Threading.Generic
{
    using System;
    using Threading;

    public class LazyDelegate<T> : TimerRule<T>
    {
        public LazyDelegate(Action<T, int> action, int dueTime, ThreadPriority priority = (ThreadPriority)2)
            : base(action, dueTime, priority)
        {
            bool_0 = false;
        }
    }
}

