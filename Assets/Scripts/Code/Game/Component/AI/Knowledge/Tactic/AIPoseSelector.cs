using System;
using System.Collections.Generic;

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
                for (int i = 0; i < poseList.Length; i++)
                {
                    res.poses.Add(poseList[i]);
                }
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