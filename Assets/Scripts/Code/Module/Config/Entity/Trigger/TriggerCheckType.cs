using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public enum TriggerCheckType
    {
        [LabelText("*Point, 坐标点")][Tooltip("只会生成简略碰撞信息，碰撞点为对方坐标点")]
        Point,
        [LabelText("*ModelHeight, 坐标点+模型高度")][Tooltip("只会生成简略碰撞信息，对方有模型信息(即为Actor且ConfigActor.Common有值)时碰撞点为自己中心点，否则为对方坐标点")]
        ModelHeight,
        [LabelText("*Collider, 身上挂载的Collider组件")][Tooltip("会生成详细碰撞信息")]
        Collider
    }
}