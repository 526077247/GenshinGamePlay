using Sirenix.OdinInspector;

namespace TaoTie
{
    public class StateData
    {
        [TabGroup("Pose")]
        public bool IsDynamic;
        [ShowIf("@IsDynamic")][TabGroup("Pose")]
        public string Key;
        [ShowIf("@!IsDynamic")][TabGroup("Pose")]
        public int PoseID;
        [LabelText("是否能移动")][TabGroup("FSM")]
        public bool CanMove;
        [LabelText("是否能转向")][TabGroup("FSM")]
        public bool CanTurn;
        [LabelText("是否展示武器")][TabGroup("FSM")]
        public bool ShowWeapon;
    }
}