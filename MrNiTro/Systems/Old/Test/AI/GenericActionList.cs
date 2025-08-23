using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Albetros.Core
{
    public enum ActionQueueType
    {
        None
    }

    public class GenericActionList<T>
    {
        private readonly IDictionary<T, Timer> _timers;

        public GenericActionList()
        {
            _timers = new Dictionary<T, Timer>();
        }
        public Timer PullAction(T type)
        {
            if (_timers.ContainsKey(type))
            {
                return _timers[type];
            }

            return null;
        }

        public Timer AddAction(T type, Action action, int delay)
        {
            lock (_timers)
            {
                if (_timers.ContainsKey(type))
                {
                    _timers[type].Change(delay, Timeout.Infinite);
                    return _timers[type];
                }

                Timer timer = null;
                timer = new Timer(state =>
                {
                    action();
                    if (timer != null)
                    {
                        timer.Dispose();
                        _timers.Remove(type);
                    }
                }, null, delay, Timeout.Infinite);
                _timers.Add(type, timer);
                return timer;
            }
        }

        public Timer AddAction(T type, Action action, int delay, int period)
        {
            if (_timers.ContainsKey(type))
            {
                _timers.Remove(type);
            }

            var timer = new Timer(state => action(), null, delay, period);
            _timers.Add(type, timer);
            return timer;
        }

        public Timer RemoveAction(T type)
        {
            if (!_timers.ContainsKey(type)) return null;
            var timer = _timers[type];
            timer.Dispose();
            lock (_timers)
                _timers.Remove(type);
            return timer;
        }
        
    }
}
