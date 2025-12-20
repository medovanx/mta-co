using System;
using System.Reflection;
using System.Threading;

internal sealed class Class4 : Class2
{
    private TimerRule timerRule_0;

    public Class4(TimerRule timerRule_1)
    {
        timerRule_0 = timerRule_1;
    }

    internal override void vmethod_0()
    {
        if (timerRule_0 != null)
        {
            timerRule_0.action_0(MTA.TTime.Now.Value);
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
    }

    internal override MethodInfo vmethod_2()
    {
        return timerRule_0.action_0.Method;
    }

    internal override ThreadPriority vmethod_3()
    {
        return timerRule_0.threadPriority_0;
    }
}

