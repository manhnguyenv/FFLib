using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Permissions;

namespace FFLib.Threading
{
   [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
   public class SynchronizationContext : System.Threading.SynchronizationContext, IDisposable
   {
      private NonBlockingQueue<SendOrPostCallbackItem> mQueue;
      //private StaThread mStaThread;
      private int _mainThreadId;
      public SynchronizationContext()
         : base()
      {
         _mainThreadId = Thread.CurrentThread.ManagedThreadId;
         mQueue = new NonBlockingQueue<SendOrPostCallbackItem>();
         //mStaThread = new StaThread(mQueue);
        // mStaThread.Start();
      }

      public void DoEvents()
      {
          SendOrPostCallbackItem workItem = mQueue.Dequeue();
          if (workItem != null)
              workItem.Execute();
      }


      public override void Send(SendOrPostCallback d, object state)
      {
         // to avoid deadlock!
         if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
         {            
            d(state);
            return;
         }

         // create an item for execution
         SendOrPostCallbackItem item = new SendOrPostCallbackItem(d, state, ExecutionType.Send);
         // queue the item
         mQueue.Enqueue(item);
         // wait for the item execution to end
         item.ExecutionCompleteWaitHandle.WaitOne();

         // if there was an exception, throw it on the caller thread, not the
         // sta thread.
         if (item.ExecutedWithException)
            throw item.Exception;         
      }

      public override void Post(SendOrPostCallback d, object state)
      {
         // queue the item and don't wait for its execution. This is risky because
         // an unhandled exception will terminate the STA thread. Use with caution.
         SendOrPostCallbackItem item = new SendOrPostCallbackItem(d, state, ExecutionType.Post);
         mQueue.Enqueue(item);
      }

      public void Dispose()
      {
         //mStaThread.Stop();
               
      }
    
      public override System.Threading.SynchronizationContext CreateCopy()
      {
         return this;
      }


   }

}
