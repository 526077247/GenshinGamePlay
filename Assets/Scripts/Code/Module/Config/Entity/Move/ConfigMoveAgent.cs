using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 移动方式基类配置。
    ///
    /// 击退参数放在基类而不是各子类各写一份：三套移动方式（动画/速度/刚体）吃的是同一套冲量口径，
    /// 各写一份的结果是「改了一边忘了另一边」，且动画与速度移动此前确实是两份完全重复的定义（5 / 50）。
    ///
    /// 字段号用 30/31：protobuf-net 把基类字段平铺进派生消息，全继承链共用一个号段，
    /// 而 ConfigSimpleMove 占了 1~4、ConfigAnimatorMove 占了 10~22，所以这里必须避开。
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigAnimatorMove))]
    [ProtoInclude(101, typeof(ConfigRigidbodyMove))]
    [ProtoInclude(102, typeof(ConfigSimpleMove))]
    public abstract partial class ConfigMoveAgent
    {
        [ProtoMember(30)][LabelText("击退速度倍率")][Tooltip("击退冲量 → 速度的倍率。0 表示该移动方式不消费击退（会打一次告警）。位移 ≈ (冲量*倍率)²/(2*衰减)")]
        public float HitImpulseScale = 5f;

        [ProtoMember(31)][LabelText("击退衰减(u/s)")][Tooltip("击退速度的线性衰减速率，竖直分量同样生效。数值越大退得越近")]
        public float HitImpulseDecay = 50f;
    }
}
