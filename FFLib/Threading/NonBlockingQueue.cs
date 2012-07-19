using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FFLib.Threading
{
  

    internal class NonBlockingQueue<T> : IQueueReader<T>, IQueueWriter<T>, IDisposable
    {
        private Queue<T> mQueue = new Queue<T>();
        //private Semaphore mSemaphore = new Semaphore(0, int.MaxValue);
        private ManualResetEvent mKillThread = new ManualResetEvent(true);
        private WaitHandle[] mWaitHandles;

        public NonBlockingQueue()
        {
            //mWaitHandles = new WaitHandle[2] { mSemaphore, mKillThread };
            mWaitHandles = new WaitHandle[1] { mKillThread };
        }
        public void Enqueue(T data)
        {
            lock (mQueue) mQueue.Enqueue(data);
            //mSemaphore.Release();
        }

        public T Dequeue()
        {
            WaitHandle.WaitAny(mWaitHandles);                  
            lock (mQueue)
            {
                if (mQueue.Count > 0)
                    return mQueue.Dequeue();
            }
            return default(T);
        }

        public void ReleaseReader()
        {
            mKillThread.Set();
        }


        void IDisposable.Dispose()
        {
            //if (mSemaphore != null)
            //{
            //    mSemaphore.Close();
                mQueue.Clear();
            //    mSemaphore = null;
            //}
        }
    }
}
