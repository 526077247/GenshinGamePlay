using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigTrigger
    {
        [NinoMember(1)][LabelText("遍历的场景单位类型")]
        public ConcernType ConcernType;
        [NinoMember(2)][LabelText("目标类型")]
        public TargetType TriggerFlag;
        [NinoMember(3)][LabelText("*触发检查精度级别")][Tooltip("一般子弹之类的涉及到攻击的必须用Collider，其他看情况")]
        public TriggerCheckType CheckType = TriggerCheckType.Collider;
        [NinoMember(9)][NotNull]
        public Vector3 Offset;
        [NinoMember(4)][NotNull]
        public ConfigShape ConfigShape;
        [NinoMember(5)][LabelText("创建后第一次开始检测时间")][MinValue(0)]
        public int StartCheckTime;
        [NinoMember(6)][LabelText("每次检查间隔")][MinValue(0)]
        public int CheckInterval;
        [NinoMember(14)]
        [LabelText("范围检测方式")]
        public CheckRangeType CheckRangeType;
        [NinoMember(7)][LabelText("*检查总次数")][Tooltip("-1：不限次")][MinValue(-1)]
        public int CheckCount = -1;
        [NinoMember(11)][LabelText("单个Entity触发间隔")][MinValue(0)]
        public int TriggerInterval;
        [NinoMember(12)][LabelText("*单个Entity触发总次数限制")][MinValue(1)]
        public uint TriggerCount = uint.MaxValue;
        [NinoMember(13)][LabelText("*所有Entity触发总次数限制")][MinValue(1)]
        public uint TotalTriggerCount = uint.MaxValue;
        [NinoMember(8)][LabelText("*存在时长")][Tooltip("最长不会超过Entity存在时长,-1：和Entity保持一致")][MinValue(-1)]
        public int LifeTime = -1;
    }
}