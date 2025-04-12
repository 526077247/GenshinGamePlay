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
        [NinoMember(3)][LabelText("触发检查精度级别")]
        public TriggerCheckType CheckType;
        [NinoMember(4)][NotNull]
        public ConfigShape ConfigShape;
        [NinoMember(5)][LabelText("创建后第一次开始检测时间")][MinValue(0)]
        public long StartCheckTime;
        [NinoMember(6)][LabelText("每次检查间隔")][MinValue(0)]
        public long CheckInterval;
        [NinoMember(7)][LabelText("*检查总次数")][Tooltip("-1：不限次")][MinValue(-1)]
        public int CheckCount = -1;
        [NinoMember(8)][LabelText("*存在时长")][Tooltip("最长不会超过Entity存在时长,-1：和Entity保持一致")][MinValue(-1)]
        public long LifeTime = -1;
    }
}