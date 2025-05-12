using System.Collections.Generic;

namespace TaoTie
{
    public static class UnityLifeTimeHelper
    {

        public static readonly Queue<ETTask> UpdateFinishTask = new Queue<ETTask>();
        //等待这一帧所有update结束
        public static ETTask WaitUpdateFinish()
        {
            ETTask task = ETTask.Create(true);
            UpdateFinishTask.Enqueue(task);
            return task;
        }
        
        public static readonly Queue<ETTask> LateUpdateFinishTask = new Queue<ETTask>();
        //等待这一帧所有lateupdate结束
        public static ETTask WaitLateUpdateFinish()
        {
            ETTask task = ETTask.Create(true);
            LateUpdateFinishTask.Enqueue(task);
            return task;
        }
        
        public static readonly Queue<ETTask> FixedUpdateFinishTask = new Queue<ETTask>();
        //等待这一帧所有fixedupdate结束
        public static ETTask WaitFixedUpdateFinish()
        {
            ETTask task = ETTask.Create(true);
            FixedUpdateFinishTask.Enqueue(task);
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