using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorExperiments.Misc
{
    public class ExperimentalTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly List<Thread> _threads;
        private readonly BlockingCollection<Task> _tasks;

        public ExperimentalTaskScheduler(int numberOfThreads) : this(numberOfThreads, Enumerable.Range(0, Environment.ProcessorCount).ToArray())
        {
        }

        public ThreadPriority? ThreadPriority { get; set; } = System.Threading.ThreadPriority.AboveNormal;

        public ExperimentalTaskScheduler(int numberOfThreads, params int[] processorIndexes)
        {
            if (numberOfThreads < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfThreads));

            foreach (var processorIndex in processorIndexes)
            {
                if (processorIndex >= Environment.ProcessorCount || processorIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(processorIndexes), $"processor index {processorIndex} was supperior to the total number of processors in the system");
            }

            _tasks = new BlockingCollection<Task>();

            _threads = Enumerable.Range(0, numberOfThreads)
                                 .Select(i => new Thread(() => ThreadStartWithAffinity(i, processorIndexes)) { IsBackground = true })
                                 .ToList();

            _threads.ForEach(t => t.Start());
        }

        private void ThreadStartWithAffinity(int threadIndex, int[] processorIndexes)
        {
            var processorIndex = processorIndexes[threadIndex % processorIndexes.Length];

            SetThreadAffinity(processorIndex);

            try
            {
                foreach (var t in _tasks.GetConsumingEnumerable())
                {
                    TryExecuteTask(t);
                }
            }
            finally
            {
                RemoveThreadAffinity();
            }
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }

        public override int MaximumConcurrencyLevel => _threads.Count;

        public void Dispose()
        {
            _tasks.CompleteAdding();
            _threads.ForEach(t => t.Join());
            _tasks.Dispose();
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        private static void SetThreadAffinity(int processorIndex)
        {
            // notify the runtime we are going to use affinity
            Thread.BeginThreadAffinity();

            // we can now safely access the corresponding native thread
            var processThread = CurrentProcessThread;

            var affinity = (1 << processorIndex);

            processThread.ProcessorAffinity = new IntPtr(affinity);
        }

        private static void RemoveThreadAffinity()
        {
            var processThread = CurrentProcessThread;

            var affinity = (1 << Environment.ProcessorCount) - 1;

            processThread.ProcessorAffinity = new IntPtr(affinity);

            Thread.EndThreadAffinity();
        }

        private static ProcessThread CurrentProcessThread
        {
            get
            {
                var threadId = GetCurrentThreadId();

                foreach (ProcessThread processThread in Process.GetCurrentProcess().Threads)
                {
                    if (processThread.Id == threadId)
                    {
                        return processThread;
                    }
                }

                throw new InvalidOperationException($"Could not retrieve native thread with ID: {threadId}, current managed thread ID was {Thread.CurrentThread.ManagedThreadId}");
            }
        }
    }
}