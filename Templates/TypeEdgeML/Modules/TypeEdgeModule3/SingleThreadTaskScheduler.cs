using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TypeEdgeModule3
{
    public sealed class SingleThreadTaskScheduler : TaskScheduler
    {
        [ThreadStatic] private static bool _isExecuting;
        private readonly CancellationToken _cancellationToken;

        private readonly BlockingCollection<Task> _taskQueue;

        public SingleThreadTaskScheduler(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _taskQueue = new BlockingCollection<Task>();
        }

        public void Start()
        {
            new Thread(RunOnCurrentThread) {Name = "STTS Thread"}.Start();
        }

        // Just a helper for the sample code
        public Task Schedule(Action action)
        {
            return
                Task.Factory.StartNew
                (
                    action,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    this
                );
        }

        // You can have this public if you want - just make sure to hide it
        private void RunOnCurrentThread()
        {
            _isExecuting = true;

            try
            {
                foreach (var task in _taskQueue.GetConsumingEnumerable(_cancellationToken)) TryExecuteTask(task);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isExecuting = false;
            }
        }

        // Signaling this allows the task scheduler to finish after all tasks complete
        public void Complete()
        {
            _taskQueue.CompleteAdding();
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override void QueueTask(Task task)
        {
            try
            {
                _taskQueue.Add(task, _cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // We'd need to remove the task from queue if it was already queued. 
            // That would be too hard.
            if (taskWasPreviouslyQueued) return false;

            return _isExecuting && TryExecuteTask(task);
        }
    }
}