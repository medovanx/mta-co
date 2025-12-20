namespace System.Threading
{
    using System;

    public class TimerRule
    {
        internal Action<int> action_0;
        internal bool bool_0;
        internal int int_0;
        internal ThreadPriority threadPriority_0;

        public TimerRule(Action<int> action, int period, ThreadPriority priority = (ThreadPriority)2)
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

