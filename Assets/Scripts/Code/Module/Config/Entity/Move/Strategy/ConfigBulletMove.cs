using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][LabelText("子弹")]
    public partial class ConfigBulletMove: ConfigMoveStrategy
    {
        [NinoMember(10)] [LabelText("初速度(m/s)")][Range(0,100000)]
        public float Speed;
        [NinoMember(11)][LabelText("最大速度(m/s)")][Range(0,100000)]
        public float MaxSpeed = 100000;
        [NinoMember(12)] [LabelText("最小速度(m/s)")][Range(0,100000)]
        public float MinSpeed;
        [NinoMember(13)][LabelText("前进方向旋转角速度(°/s)")]
        public BaseValue AnglerVelocity = new ZeroValue();
        [NinoMember(14)][LabelText("加速度(m/s)")]
        public BaseValue Acceleration = new SingleValue();
        [NinoMember(15)][LabelText("加速时间(ms)")][Tooltip("<0无限；>=0加速多久")]
        public int AccelerationTime;
        [NinoMember(16)][LabelText("*延迟启动(ms)")][Tooltip("<0不启动；0立刻；>0延迟多久")]
        public int Delay;
        
    }
}