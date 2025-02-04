using System.Collections.Generic;

namespace TaoTie
{
    public static class WaitHelper
    {
        public static readonly Queue<ETTask> UpdateFinishTask = new Queue<ETTask>();
        //等待这一帧所有update结束
        public static ETTask WaitUpdateFinish()
        {
            ETTask task = ETTask.Create(true);
            UpdateFinishTask.Enqueue(task);
            return task;
        }
        
        public static readonly Queue<ETTask> FrameFinishTask = new Queue<ETTask>();
        public static ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            FrameFinishTask.Enqueue(task);
            return task;
        }
    }
}