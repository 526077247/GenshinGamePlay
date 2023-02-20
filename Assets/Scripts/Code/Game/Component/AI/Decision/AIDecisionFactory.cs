namespace TaoTie
{
	public static class AIDecisionFactory
	{
		public static void OnAlert(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.OnAlert;
			decision.move = MoveDecision.StandStill;
			decision.act = ActDecision.OnAlert;
		}

		public static void OnAware(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.OnAware;
			decision.move = MoveDecision.StandStill;
			decision.act = ActDecision.OnAware;
		}

		public static void OnNerve(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatBuddySkill(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatSkillExecute(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.CombatSkill;
			decision.move = MoveDecision.StandStill;
			decision.act = ActDecision.CombatSkill;
		}

		public static void CombatSkillPrepare(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.CombatSkillPrepare;
			decision.move = MoveDecision.SkillPrepare;
			decision.act = ActDecision.CombatSkillPrepare;
		}

		public static void CombatFixedMove(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatSpacialChase(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatMeleeCharge(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.MeleeCharge;
			decision.move = MoveDecision.MeleeCharge;
			decision.act = ActDecision.NoActDecision;
		}

		public static void CombatFollowMove(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatFacingMove(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.FacingMove;
			decision.move = MoveDecision.FacingMove;
			decision.act = ActDecision.NoActDecision;
		}

		public static void CombatSurround(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatFindBack(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatCrabMove(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatSpacialAdjust(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void CombatIdle(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.CombatIdle;
			decision.move = MoveDecision.StandStill;
			decision.act = ActDecision.NoActDecision;
		}

		public static void ReturnToBornPos(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.ReturnToBornPos;
			decision.move = MoveDecision.ReturnToBorn;
			decision.act = ActDecision.NoActDecision;
			// knowledge.actionControlState.status = AISkillStatus.Fail;
		}

		public static void Investigate(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void ReactActionPoint(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void FreeSkill(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void FollowServerRoute(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void PatrolFollow(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void FollowScriptedPath(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void ScriptedMoveTo(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void SpacialProbe(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void Wander(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.Wander;
			decision.move = MoveDecision.Wander;
			decision.act = ActDecision.NoActDecision;
		}

		public static void Landing(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void Extraction(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void Flee(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.Flee;
			decision.move = MoveDecision.Flee;
			decision.act = ActDecision.NoActDecision;
		}

		public static void BirdCircling(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void Idle(AIKnowledge knowledge, AIDecision decision)
		{
			decision.tactic = AITactic.Idle;
			decision.move = MoveDecision.StandStill;
			decision.act = ActDecision.NoActDecision;
		}

		public static void AutoPlayerFollowTarget(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void AutoPlayerCombatSkillExecute(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void AutoPlayerCombatSkillPrepare(AIKnowledge knowledge, AIDecision decision)
		{
		}

		public static void BrownianMotion(AIKnowledge knowledge, AIDecision decision)
		{
		}
	}
}