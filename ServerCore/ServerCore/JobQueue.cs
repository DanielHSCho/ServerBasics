using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    public class JobQueue : IJobQueue
    {
        // 내가 해야하는 일감들을 가지고 있는 큐
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();

        public void Push(Action job)
        {
            lock (_lock) {
                _jobQueue.Enqueue(job);
            }
        }

        private Action Pop()
        {
            lock (_lock) {
                if(_jobQueue.Count == 0) {
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
