namespace TaoTie
{
    public class HardLockToTargetStateData
    {
        public float damping;

        public HardLockToTargetStateData(ConfigHardLockToTarget config)
        {
            if (config == null)
            {
                config = new ConfigHardLockToTarget();
            }

            damping = config.damping;
        }
    }
}