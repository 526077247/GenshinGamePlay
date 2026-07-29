using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigTrigger
    {
        [ProtoMember(1)][LabelText("遍历的场景单位类型")]
        public ConcernType ConcernType;
        [ProtoMember(2)][LabelText("目标类型")]
        public TargetType TriggerFlag;
        [ProtoMember(3, IsRequired = true)][LabelText("*触发检查精度级别")][Tooltip("一般子弹之类的涉及到攻击的必须用Collider，其他看情况")]
        public TriggerCheckType CheckType = TriggerCheckType.Collider;
        [ProtoMember(9)][NotNull]
        public Vector3 Offset;
        [ProtoMember(4)][NotNull]
        public ConfigShape ConfigShape;
        [ProtoMember(5)][LabelText("创建后第一次开始检测时间")][MinValue(0)]
        public int StartCheckTime;
        [ProtoMember(6)][LabelText("每次检查间隔")][MinValue(0)]
        public int CheckInterval;
        [ProtoMember(14)]
        [LabelText("范围检测方式")]
        public CheckRangeType CheckRangeType;
        [ProtoMember(7, IsRequired = true)][LabelText("*检查总次数")][Tooltip("-1：不限次")][MinValue(-1)]
        public int CheckCount = -1;
        [ProtoMember(11)][LabelText("单个Entity触发间隔")][MinValue(0)]
        public int TriggerInterval;
        [ProtoMember(12, IsRequired = true)][LabelText("*单个Entity触发总次数限制")][MinValue(1)]
        public uint TriggerCount = uint.MaxValue;
        [ProtoMember(13, IsRequired = true)][LabelText("*所有Entity触发总次数限制")][MinValue(1)]
        public uint TotalTriggerCount = uint.MaxValue;
        [ProtoMember(8, IsRequired = true)][LabelText("*存在时长")][Tooltip("最长不会超过Entity存在时长,-1：和Entity保持一致")][MinValue(-1)]
        public int LifeTime = -1;
    }
}