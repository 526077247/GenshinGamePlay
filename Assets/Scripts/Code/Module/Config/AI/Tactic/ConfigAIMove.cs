using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigAIMove
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool Enable = true;
        [NinoMember(2)]
        public MoveCategoryAI MoveCategory;
        [NinoMember(5)][LabelText("步行时到达判定距离")]
        public float AlmostReachedDistanceWalk;
        [NinoMember(6)][LabelText("跑步时到达判定距离")]
        public float AlmostReachedDistanceRun;
        // public ConfigAISnakelikeMove SnakelikeMoveSetting;
    }
}