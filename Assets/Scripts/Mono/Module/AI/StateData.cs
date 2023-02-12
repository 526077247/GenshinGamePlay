using Sirenix.OdinInspector;

namespace TaoTie
{
    public class StateData
    {
        [BoxGroup("Pose")]
        public bool IsDynamic;
        [ShowIf("@IsDynamic")][BoxGroup("Pose")]
        public string Key;
        [ShowIf("@!IsDynamic")][BoxGroup("Pose")]
        public int PoseID;
    }
}