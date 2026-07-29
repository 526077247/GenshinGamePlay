using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAIMove
    {
        [LabelText("启用")]
        [ProtoMember(1, IsRequired = true)]
        public bool Enable = true;
        [ProtoMember(2)]
        public MoveCategoryAI MoveCategory;
        [ProtoMember(5)][LabelText("步行时到达判定距离")]
        public float AlmostReachedDistanceWalk;
        [ProtoMember(6)][LabelText("跑步时到达判定距离")]
        public float AlmostReachedDistanceRun;
        // public ConfigAISnakelikeMove SnakelikeMoveSetting;
    }
}