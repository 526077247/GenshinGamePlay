using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum CheckRangeType
    {
        [LabelText("None, 当前帧位置")]
        None,
        [LabelText("*Lerp, 和前一次检测位置间插值")][Tooltip("每间隔0.1m检测一次，最多10次。超过10次时，间隔为距离/10")]
        Lerp,
        [LabelText("Raycast, 和前一次检测位置间射线检测")]
        Raycast,
        [LabelText("EachModel, 检测当前帧的每一个模型位置")]
        EachModel,
    }
}