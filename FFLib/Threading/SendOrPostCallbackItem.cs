﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FFLib.Threading
{
   internal enum ExecutionType
   {
      Post,
      Send
   }
   
   internal class SendOrPostCallbackItem
   {
      object mState;
      private ExecutionType mExeType;
      SendOrPostCallback mMethod;
      ManualResetEvent mAsyncWaitHandle = new ManualResetEvent(false);
      Exception mException = null;
     
      internal SendOrPostCallbackItem(SendOrPostCallback callback, object state, ExecutionType type)
      {
         mMethod = callback;
         mState = state;
         mExeType = type;
      }

      internal Exception Exception
      {
         get { return mException; }
      }

      internal bool ExecutedWithException
      {
         get { return mException != null; }
      }

      // this code must run ont the STA thread
      internal void Execute()
      {
         if (mExeType == ExecutionType.Send)
            Send();
         else
            Post();
      }

      // calling thread will block until mAsyncWaitHanel is set
      internal void Send()
      {
         try
         {
            // call the thread
            mMethod(mState);            
         }
         catch (Exception e)
         {
            mException = e;
         }
         finally
         {
            mAsyncWaitHandle.Set();
         }
      }

      /// <summary>
      /// Unhandle execptions will terminate the STA thread
      /// </summary>
      internal void Post()
      {
         mMethod(mState);
      }

      internal WaitHandle ExecutionCompleteWaitHandle
      {
         get { return mAsyncWaitHandle; }
      }
   }
}
