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
        public TriggerCheckType CheckType = TriggerCheckType.ModelHeight;
        [NinoMember(9)][NotNull]
        public Vector3 Offset;
        [NinoMember(4)][NotNull]
        public ConfigShape ConfigShape;
        [NinoMember(5)][LabelText("创建后第一次开始检测时间")][MinValue(0)]
        public int StartCheckTime;
        [NinoMember(6)][LabelText("每次检查间隔")][MinValue(0)]
        public int CheckInterval;
        [NinoMember(14)]
        [LabelText("*启用射线检测")][Tooltip("当每次检查间隔很短(<200ms)时可开启，开启后会在两次检测位置间进行射线检查")]
        [ShowIf(nameof(CanOpenRaycastRoute))]
        public bool RaycastCheck = false;
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
        [NinoMember(10)] [LabelText("*应用到每一个模型")][Tooltip("当模型不为单实例时有效")]
        public bool ApplyEachModel = true;
        
        
        public bool CanOpenRaycastRoute()
        {
            return CheckInterval < Define.MinRepeatedTimerInterval*2;
        }
    }
}