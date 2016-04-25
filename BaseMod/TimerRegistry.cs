using System.Collections.Generic;

namespace BaseMod
{
    public class TimerRegistry
    {
        private static readonly HashSet<ThreadsafeTimer> Timers = new HashSet<ThreadsafeTimer>();
        private static readonly HashSet<ThreadsafeTimer> TimersToAdd = new HashSet<ThreadsafeTimer>();
        private static readonly HashSet<ThreadsafeTimer> TimersToRemove = new HashSet<ThreadsafeTimer>();

        private static bool _isUpdating;

        public static void Add(ThreadsafeTimer timer)
        {
            if (_isUpdating)
                TimersToAdd.Add(timer);
            else
                Timers.Add(timer);
        }

        public static void Remove(ThreadsafeTimer timer)
        {
            if (_isUpdating)
                TimersToRemove.Add(timer);
            else
                Timers.Remove(timer);
        }

        public static void Update()
        {
            _isUpdating = true;

            foreach (var timer in Timers)
                timer.Update();

            _isUpdating = false;

            foreach (var threadsafeTimer in TimersToAdd)
                Timers.Add(threadsafeTimer);

            TimersToAdd.Clear();

            foreach (var threadsafeTimer in TimersToRemove)
                Timers.Remove(threadsafeTimer);

            TimersToRemove.Clear();
        }

        public static void Close()
        {
            var tmp = new HashSet<ThreadsafeTimer>(Timers);
            // clear the set to avoid exceptions as the timers will try to remove themselves from the _timers set
            // wouldn't be smart to do that while iterating trough the mentioned set...
            Timers.Clear();
            foreach (var timer in tmp)
            {
                timer.Close();
            }
        }
    }
}
