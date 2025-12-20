using System;
using System.Reflection;
using System.Threading;
using System.Threading.Generic;

internal sealed class Class3<T> : Class2
{
    private T gparam_0;
    private TimerRule<T> timerRule_0;

    public Class3(TimerRule<T> timerRule_1, T gparam_1)
    {
        timerRule_0 = timerRule_1;
        gparam_0 = gparam_1;
    }

    internal override void vmethod_0()
    {
        if (timerRule_0 != null)
        {// lol :D old threading system :S
            timerRule_0.action_0(gparam_0, MTA.TTime.Now.Value);
            if (timerRule_0 != null)
            {
                if (!timerRule_0.bool_0)
                {
                    ((IDisposable)this).Dispose();
                }
                else
                {
                    method_1(timerRule_0.int_0);
                }
            }
        }
    }

    internal override void vmethod_1()
    {
        timerRule_0 = null;
        gparam_0 = default(T);
    }

    internal override MethodInfo vmethod_2()
    {
        return timerRule_0.action_0.Method;
    }
    internal override ThreadPriority vmethod_3()
    {
        if (timerRule_0 == null)
            return ThreadPriority.Normal;
        return timerRule_0.threadPriority_0;
    }
}

