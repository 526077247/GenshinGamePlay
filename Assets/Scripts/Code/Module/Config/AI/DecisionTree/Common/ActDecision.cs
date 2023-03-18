using Sirenix.OdinInspector;

namespace TaoTie
{
    public enum ActDecision
    {
        [LabelText("无")]
        NoActDecision = 0,
        [LabelText("当感知")]
        OnAware = 1,
        [LabelText("当警惕")]
        OnAlert = 2,
        [LabelText("狂暴")]
        OnNerve = 3,
        [LabelText("自由技能")]
        FreeSkill = 4,
        [LabelText("对地点技能")]
        ActionPointSkill = 5,
        [LabelText("战斗技能")]
        CombatSkill = 6,
        [LabelText("战斗技能预备")]
        CombatSkillPrepare = 7,
        [LabelText("瞄准")]
        Aiming = 8,
        [LabelText("固定移动点技能")]
        FixedMovePointSkill = 9,
        [LabelText("对伙伴使用技能")]
        BuddySkill = 10
    }
}