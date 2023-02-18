using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigAIMove
    {
        [LabelText("启用")]
        [NinoMember(1)]
        public bool enable;
        [NinoMember(2)]
        public MoveCategoryAI moveCategory;
        [NinoMember(5)][LabelText("步行时到达判定距离")]
        public float almostReachedDistanceWalkRawNum;
        [NinoMember(6)][LabelText("跑步时到达判定距离")]
        public float almostReachedDistanceRunRawNum;
        // public ConfigAISnakelikeMove _snakelikeMoveSetting;
    }
}