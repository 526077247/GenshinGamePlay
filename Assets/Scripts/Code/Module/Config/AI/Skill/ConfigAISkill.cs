using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigAISkill
    {
        [ProtoMember(1)][LabelText("配置表Id")]
        public int ConfigId;
        [ProtoMember(20, IsRequired = true)]
        public bool Enable = true;
        [ProtoMember(2)]
        public ConfigAISkillType SkillType;
        [ProtoMember(3)][LabelText("随机权值")]
        public int Weights;
        [ProtoMember(4)][LabelText("需要进入可视范围")]
        public bool NeedLineOfSight;
        [ProtoMember(5)] [LabelText("释放时朝向目标？")]
        public bool FaceTarget;
        [ProtoMember(6)] [LabelText("目标无效时是否可使用？")]
        public bool CanUseIfTargetInactive;
        /// <summary>
        /// CD有4块，aimanager管理的publiccd-全场AI通用，gcd-该ai内部通用，GroupCDID该ai内部指定组通用，cd-skill的cd
        /// </summary>

        #region 技能cd

        [ProtoMember(7, IsRequired = true)][BoxGroup("技能cd")][MinValue(100)]
        public int CD = 1000;
        [ProtoMember(8)][LabelText("技能cd增长随机最大值")][BoxGroup("技能cd")][MinValue(0)]
        public int CdUpperRange;

        #endregion

        #region aimanager管理的publiccd-全场AI通用

        [ProtoMember(9)][LabelText("场景公共CD")][BoxGroup("场景公共CD")]
        public string PublicCDGroup;

        #endregion

        #region 该ai内部公共cd
        [ProtoMember(10)][LabelText("忽略单位CD？")][BoxGroup("单位CD")]
        public bool IgnoreGCD;
        [ProtoMember(11, IsRequired = true)][LabelText("单位CD是否需要进入冷却")][ShowIf("@!"+nameof(IgnoreGCD))][BoxGroup("单位CD")]
        public bool TriggerGCD = true;
        
        [ProtoMember(12)] [LabelText("单位CD组时长配置id")][BoxGroup("单位CD")]
        public int SkillGroupCDID;
        #endregion
        
        [ProtoMember(13)][LabelText("*该技能包含的State")][Tooltip("不处于这些状态中时算技能释放完成")]
        public string[] StateIds;
        [ProtoMember(14)][LabelText("技能开始时就触发冷却")]
        public bool TriggerCDOnStart;
        [ProtoMember(15)][LabelText("技能使用条件判断")]
        public ConfigAISkillCastCondition CastCondition;

        [ProtoMember(16, IsRequired = true)][BoxGroup("技能准备")]
        public bool EnableSkillPrepare = true;
        [ProtoMember(17, IsRequired = true)][BoxGroup("技能准备")]
        public int SkillPrepareTimeout = 1000;
        [ProtoMember(18, IsRequired = true)][BoxGroup("技能准备")]
        public MotionFlag SkillPrepareSpeedLevel = MotionFlag.Walk;
        [ProtoMember(19, IsRequired = true)][BoxGroup("技能准备")]
        public int SkillQueryingTime = 1000;
    }
}