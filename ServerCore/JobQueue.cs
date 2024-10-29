namespace ServerCore
{
    public interface IJobQueue
    {
        public void Push(Action job);
        public Action Pop();
    }

    public class JobQueue : IJobQueue
    {
        Queue<Action> jobQueue = new Queue<Action>();
        bool _shouldFlush = false;

        object lockObject = new object();

        public void Push(Action job)
        {
            bool shouldFlush = false;

            lock (lockObject)
            {
                jobQueue.Enqueue(job);
                if (_shouldFlush == false)
                    shouldFlush = _shouldFlush = true;
            }

            if (shouldFlush)
                Flush();
        }

        void Flush()
        {
            while (true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        public Action Pop()
        {
            lock (lockObject)
            {
                if (jobQueue.Count == 0)
                {
                    _shouldFlush = false;
                    return null;
                }

                return jobQueue.Dequeue();
            }
        }
    }
}