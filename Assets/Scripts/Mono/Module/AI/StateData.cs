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
        [LabelText("是否能移动")]
        public bool CanMove;
        [LabelText("是否能转向")]
        public bool CanTurn;
    }
}