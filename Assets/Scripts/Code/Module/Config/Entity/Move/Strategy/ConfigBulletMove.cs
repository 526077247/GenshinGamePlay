using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract][LabelText("子弹")]
    public partial class ConfigBulletMove: ConfigMoveStrategy
    {
        [ProtoMember(10)] [LabelText("初速度(m/s)")][Range(0,100000)]
        public float Speed;
        [ProtoMember(11, IsRequired = true)][LabelText("最大速度(m/s)")][Range(0,100000)]
        public float MaxSpeed = 100000;
        [ProtoMember(12)] [LabelText("最小速度(m/s)")][Range(0,100000)]
        public float MinSpeed;
        [ProtoMember(13, IsRequired = true)][LabelText("前进方向旋转角速度(°/s)")]
        public BaseValue AnglerVelocity = new ZeroValue();
        [ProtoMember(14, IsRequired = true)][LabelText("加速度(m/s)")]
        public BaseValue Acceleration = new SingleValue();
        [ProtoMember(15)][LabelText("加速时间(ms)")][Tooltip("<0无限；>=0加速多久")]
        public int AccelerationTime;
        [ProtoMember(16)][LabelText("*延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")]
        public int Delay;
        
    }
}