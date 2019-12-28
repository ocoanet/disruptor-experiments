using System;
using System.Threading;

namespace TaskExperiments.Publisher.Custom
{
    public class AwaitableEvent
    {
        private SpinLock _spinLock = new SpinLock();
        private Action _continuation;
        private long _completedSequence = -1;

        /// <summary>
        /// Single-threaded, one call per sequence, sequential.
        /// </summary>
        internal void SignalCompletion(long sequence)
        {
            Action continuation;
            
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                continuation = _continuation;
                
                _completedSequence = sequence;
                _continuation = null;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
            
            continuation?.Invoke();
        }

        internal bool IsCompleted(long sequence)
        {
            return sequence <= _completedSequence;
        }

        /// <summary>
        /// Multi-threaded, one call per sequence, out-of-order.
        ///
        /// SignalStart(s1) cannot happen before SignalCompletion(s0).
        /// </summary>
        internal void SignalStart(long sequence)
        {
        }

        /// <summary>
        /// Multi-threaded, zero or one call per sequence, out-of-order.
        ///
        /// RegisterContinuation(s1) cannot happen before SignalCompletion(s0).
        /// RegisterContinuation(s1) cannot happen before SignalStart(s1).
        /// </summary>
        internal void RegisterContinuation(long sequence, Action continuation)
        {
            bool invokeSynchronously;
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                invokeSynchronously = sequence <= _completedSequence;
                if (!invokeSynchronously)
                    _continuation = continuation;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
            
            if (invokeSynchronously)
                continuation.Invoke();
        }
    }
}