namespace System.Threading
{
    using System;
    using System.Collections.Generic;
    using Security;
    using Generic;

    public class StaticPool : IDisposable
    {
        internal volatile bool bool_0;
        internal volatile bool bool_1;
        internal Dictionary<int, Class2> dictionary_0;
        internal int int_0;
        internal int int_1;
        internal int int_2;
        internal int int_3;
        internal int int_4;
        internal List<Thread> list_0;
        internal object object_0;
        internal object object_1;
        protected internal Thread propagationThread;
        internal Queue<Class2> queue_0;
        public const int SleepTime = 1;

        public StaticPool(int maximumPoolSize = 0x20)
        {
            //   Class1.Class0.smethod_0();
            bool_1 = false;
            object_1 = new object();
            object_0 = new object();
            dictionary_0 = new Dictionary<int, Class2>();
            queue_0 = new Queue<Class2>();
            list_0 = new List<Thread>();
            int_3 = maximumPoolSize;
            int_4 = maximumPoolSize;
        }

        public void Clear()
        {
            lock (object_0)
            {
                queue_0.Clear();
            }
        }

        ~StaticPool()
        {
            method_1(false);
        }

        [SecuritySafeCritical]
        internal void method_0()
        {
            if (!bool_1)
            {
                Interlocked.Increment(ref int_0);
                Interlocked.Increment(ref int_1);
                Thread item = new Thread(new ThreadStart(method_2), 0x100000);
                list_0.Add(item);
                item.Priority = ThreadPriority.Normal;
                item.Start();
            }
        }

        internal void method_1(bool bool_2)
        {
            if (!bool_1)
            {
                bool_1 = true;
                bool_0 = false;
                if (bool_2)
                {
                    foreach (Thread thread in list_0)
                    {
                        thread.Abort();
                    }
                }
                dictionary_0.Clear();
                dictionary_0 = null;
                queue_0 = null;
                list_0 = null;
            }
        }

        internal void method_2()
        {
            Thread currentThread = Thread.CurrentThread;
            while (bool_0)
            {
                Class2 class2;
                Thread.Sleep(1);
                if (method_3(out class2))
                {
                    if (class2.bool_0)
                    {
                        Interlocked.Decrement(ref int_1);
                        Interlocked.Increment(ref int_2);
                        currentThread.Priority = class2.vmethod_3();
                        try
                        {
                            class2.vmethod_0();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        finally
                        {
                            class2.bool_1 = false;
                        }
                        currentThread.Priority = ThreadPriority.Normal;
                        Interlocked.Decrement(ref int_2);
                        Interlocked.Increment(ref int_1);
                    }
                    else
                    {
                        method_4(class2.GetHashCode());
                    }
                }
            }
            Interlocked.Decrement(ref int_1);
        }

        internal bool method_3(out Class2 class2_0)
        {
            class2_0 = null;
            lock (object_0)
            {
                if (queue_0.Count != 0)
                {
                    Class2 class2 = queue_0.Dequeue();
                    class2_0 = class2;
                }
            }
            return (class2_0 != null);
        }

        internal void method_4(int int_5)
        {
            lock (object_1)
            {
                dictionary_0.Remove(int_5);
            }
        }

        private void method_5()
        {
            while (bool_0)
            {
                Queue<Class2> queue = new Queue<Class2>();
                Queue<int> queue2 = new Queue<int>();
                lock (object_1)
                {
                    foreach (Class2 class2 in dictionary_0.Values)
                    {
                        if (class2.bool_0)
                        {
                            if (!class2.bool_1 && class2.method_0())
                            {
                                class2.bool_1 = true;
                                queue.Enqueue(class2);
                            }
                        }
                        else
                        {
                            queue2.Enqueue(class2.GetHashCode());
                        }
                    }
                    while (queue2.Count != 0)
                    {
                        dictionary_0.Remove(queue2.Dequeue());
                    }
                }
                if (queue.Count != 0)
                {
                    lock (object_0)
                    {
                        while (queue.Count != 0)
                        {
                            queue_0.Enqueue(queue.Dequeue());
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        public StaticPool Run()
        {
            bool_0 = true;
            for (int i = 0; i < int_3; i++)
            {
                method_0();
            }
            propagationThread = new Thread(new ThreadStart(method_5));
            propagationThread.Start();
            return this;
        }

        public IDisposable Subscribe(TimerRule instruction)
        {
            Class2 class2 = null;
            lock (object_1)
            {
                class2 = new Class4(instruction);
                if (instruction is LazyDelegate)
                {
                    class2.method_1(instruction.int_0);
                }
                dictionary_0[class2.GetHashCode()] = class2;
            }
            return class2;
        }

        public IDisposable Subscribe<T>(TimerRule<T> instruction, T param)
        {
            Class2 class2 = null;
            lock (object_1)
            {
                class2 = new Class3<T>(instruction, param);
                if (instruction is LazyDelegate<T>)
                {
                    class2.method_1(instruction.int_0);
                }
                dictionary_0[class2.GetHashCode()] = class2;
            }
            return class2;
        }

        void IDisposable.Dispose()
        {
            method_1(true);
        }

        public override string ToString()
        {
            int count = dictionary_0.Count;
            int num = queue_0.Count;
            return string.Format("{0} waiting exec, {1} subscriptions, {2} threads: {3} in use, {4} idle", new object[] { num, count, int_0, int_2, int_1 });
        }

        public int IdleThreads
        {
            get
            {
                return int_1;
            }
        }

        public int InUseThreads
        {
            get
            {
                return int_2;
            }
        }

        public int Threads
        {
            get
            {
                return int_0;
            }
        }

        public int Treshold
        {
            get
            {
                return queue_0.Count;
            }
        }
    }
}

