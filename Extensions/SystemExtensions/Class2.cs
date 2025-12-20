using System;
using System.Reflection;
using System.Threading;

internal abstract class Class2 : IDisposable
{
    internal bool bool_0;
    internal bool bool_1;
    internal static volatile int int_0 = -2147483648;
    protected int int_1;
    internal Time32 time32_0;

    public Class2()
    {
        int_0++;
        int_1 = int_0;
        bool_0 = true;
        bool_1 = false;
        method_1(0);
    }

    ~Class2()
    {
        ((IDisposable)this).Dispose();
    }

    public override int GetHashCode()
    {
        return int_1;
    }

    internal bool method_0()
    {
        return (Time32.Now > time32_0);
    }

    internal void method_1(int int_2)
    {
        time32_0 = Time32.Now.AddMilliseconds(int_2);
    }

    void IDisposable.Dispose()
    {
        bool_0 = false;
        vmethod_1();
    }

    internal abstract void vmethod_0();
    internal abstract void vmethod_1();
    internal abstract MethodInfo vmethod_2();
    internal abstract ThreadPriority vmethod_3();
}

