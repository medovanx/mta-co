namespace System.Threading.Generic
{
    using System;
    using Threading;

    public class TimerRule<T>
    {
        internal Action<T, int> action_0;
        internal bool bool_0;
        internal int int_0;
        internal ThreadPriority threadPriority_0;

        public TimerRule(Action<T, int> action, int period, ThreadPriority priority = (ThreadPriority)2)
        {
            action_0 = action;
            int_0 = period;
            bool_0 = true;
            threadPriority_0 = priority;
        }

        ~TimerRule()
        {
            action_0 = null;
        }
    }
}

