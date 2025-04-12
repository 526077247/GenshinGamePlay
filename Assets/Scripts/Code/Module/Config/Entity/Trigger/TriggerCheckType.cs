using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum TriggerCheckType
    {
        [LabelText("Point, 坐标点")]
        Point,
        [LabelText("ModelHeight, 坐标点+模型高度")]
        ModelHeight,
        [LabelText("Collider, 身上挂载的Collider组件")]
        Collider
    }
}