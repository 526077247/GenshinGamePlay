using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace TaoTie
{
    public class AIPoseSelector: IDisposable
    {
        private HashSetComponent<int> poses;

        public static AIPoseSelector Create(int[] poseList)
        {
            AIPoseSelector res = ObjectPool.Instance.Fetch<AIPoseSelector>();
            if (poseList != null)
            {
                res.poses = HashSetComponent<int>.Create();
                res.poses.AddRange(poseList);
            }
            return res;
        }
        public void Dispose()
        {
            poses?.Dispose();
            poses = null;
        }

        public bool CheckValidPose(int pose)
        {
            return poses == null || poses.Contains(pose);
        }
    }
}