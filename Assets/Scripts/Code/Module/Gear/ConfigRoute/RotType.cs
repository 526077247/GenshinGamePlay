using Sirenix.OdinInspector;

namespace TaoTie
{
    public enum RotType
    {
        [LabelText("不旋转")]
        ROT_NONE = 0,
        [LabelText("按角速度旋转")]
        ROT_ANGLE = 1,
        [LabelText("旋转指定圈数")]
        ROT_ROUND = 2,
        [LabelText("自动旋转朝向下一点")]
        ROT_AUTO = 3,
    }
}