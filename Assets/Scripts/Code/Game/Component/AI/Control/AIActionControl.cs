using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 行动
    /// </summary>
    public class AIActionControl : AIBaseControl
    {
        // private AIActionControlState actionState;
        private List<AISkillInfo> validCandidates;
        private AIComponent component;

        protected override void InitInternal()
        {
            base.InitInternal();
            component = aiKnowledge.aiOwnerEntity.GetComponent<AIComponent>();
        }

        public void ExecuteAction(AIDecision decision)
        {
        }

        private void SelectSkill(AISkillInfo skill)
        {
        }

        private void SelectSkill(List<AISkillInfo> skillCandidates, bool skipPrepare = true)
        {
        }

        private void CastSkill()
        {
        }

        private void OnSkillStart()
        {
        }

        private void OnSkillFinish()
        {
        }

        private void OnSkillFail(bool needTriggerCD = true)
        {
        }

        public void Clear()
        {

        }
    }
}