using UnityEngine;

namespace TaoTie
{
    public class TransposerStateData
    {
        public Vector3 followOffset;
        public float xDamping;
        public float yDamping;
        public float zDamping;
        public float yawDamping;

        public TransposerStateData(){}
        public TransposerStateData(ConfigTransposer config)
        {
            if (config == null)
            {
                config = new ConfigTransposer();
            }

            followOffset = config.FollowOffset;
            xDamping = config.XDamping;
            yDamping = config.YDamping;
            zDamping = config.ZDamping;
            yawDamping = config.YawDamping;
        }
    }
}