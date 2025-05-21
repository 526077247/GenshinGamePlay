using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum CheckRangeType
    {
        [LabelText("None, 当前帧位置")]
        None,
        [LabelText("*Lerp, 和前一次检测位置间插值")]
        [Tooltip("每间隔一定距离检测一次,最多10次。间隔距离为包围盒最宽处大小(低于0.1米按0.1米计算)。计算出来的检测间隔次数超过10次时,间隔为总距离/10")]
        Lerp,
        [LabelText("Raycast, 和前一次检测位置间射线检测")]
        Raycast,
        [LabelText("EachModel, 检测当前帧的每一个模型位置")]
        EachModel,
    }
}