using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAISkill
    {
        [NinoMember(1)]
        public int SkillID;
        [NinoMember(2)]
        public ConfigAISkillType SkillType;
        [NinoMember(3)][LabelText("优先级")]
        public int Priority;
        [NinoMember(4)][LabelText("需要进入可视范围")]
        public bool NeedLineOfSight;
        [NinoMember(5)] [LabelText("释放时朝向目标？")]
        public bool FaceTarget;
        [NinoMember(6)] [LabelText("目标无效时是否可使用？")]
        public bool CanUseIfTargetInactive;
        /// <summary>
        /// CD有4块，aimanager管理的publiccd-全场AI通用，gcd-该ai内部通用，GroupCDID该ai内部指定组通用，cd-skill的cd
        /// </summary>

        #region 自身cd

        [NinoMember(7)][BoxGroup("自身cd")][Min(100)]
        public int CD = 10000;
        [NinoMember(8)][LabelText("自身cd增长随机最大值")][BoxGroup("自身cd")]
        public int CdUpperRange;

        #endregion

        #region aimanager管理的publiccd-全场AI通用

        [NinoMember(9)][LabelText("全场公共CD")][BoxGroup("全场公共CD")]
        public string PublicCDGroup;

        #endregion

        #region 该ai内部公共cd
        [NinoMember(10)][LabelText("忽略公共CD？")][BoxGroup("公共CD")][BoxGroup("AI公共CD")]
        public bool IgnoreGCD;
        [NinoMember(11)][LabelText("公共CD是否需要进入冷却")][ShowIf("@!IgnoreGCD")][BoxGroup("公共CD")][BoxGroup("AI公共CD")]
        public bool TriggerGCD;
        #endregion
        
        #region 该ai内部公共组cd
        [NinoMember(12)] [LabelText("公共CD时长配置id")][BoxGroup("AI公共CD组")]
        public int SkillGroupCDID;
        #endregion
        
        [NinoMember(13)][LabelText("该技能包含的State")][Tooltip("不处于这些状态中时算技能释放完成")]
        public string[] StateIds;
        [NinoMember(14)][LabelText("技能开始时就触发冷却")]
        public bool TriggerCDOnStart;
        [NinoMember(15)][LabelText("技能使用条件判断")]
        public ConfigAISkillCastCondition CastCondition;
    }
}