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

            followOffset = config.followOffset;
            xDamping = config.xDamping;
            yDamping = config.yDamping;
            zDamping = config.zDamping;
            yawDamping = config.yawDamping;
        }
    }
}