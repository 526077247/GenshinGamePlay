using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace TaoTie
{
	public class AIDecisionInterface
	{
		public static Dictionary<string, Func<AIKnowledge, bool>> Methods;
		static AIDecisionInterface()
		{
			Methods = new Dictionary<string, Func<AIKnowledge, bool>>();
			var methodInfos = TypeInfo<AIDecisionInterface>.Type.GetMethods();
			for (int i = 0; i < methodInfos.Length; i++)
			{
				if (!methodInfos[i].IsStatic)
				{
					continue;
				}
				var func = (Func<AIKnowledge, bool>)Delegate.CreateDelegate(TypeInfo<Func<AIKnowledge, bool>>.Type, null, methodInfos[i]);
				Methods.Add(methodInfos[i].Name,func);
			}
		}

		public static bool IsOnAwareValid(AIKnowledge knowledge)
		{
			return knowledge.skillKnowledge.SkillsOnAware.AvailableSkills.Count > 0;
		}

		public static bool IsOnAlertValid(AIKnowledge knowledge)
		{
			return knowledge.skillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0;
		}

		public static bool IsOnNerveValid(AIKnowledge knowledge)
		{
			return knowledge.skillKnowledge.SkillsOnNerve.AvailableSkills.Count > 0;
		}
		public static bool IsReturnToBornPosValid(AIKnowledge knowledge)
		{
			if (!knowledge.moveKnowledge.canMove) return false;
			// if (!knowledge.returnToBornKnowledge.config.enable)
			// 	return false;
			// if (!knowledge.isReturnToBorn)
			// 	return false;
			return true;
		}

		public static bool IsInvestigateValid(AIKnowledge knowledge) => default;
		public static bool IsReactActionPointValid(AIKnowledge knowledge) => default;
		public static bool IsPatrolFollowValid(AIKnowledge knowledge) => default;
		public static bool IsFollowServerRouteValid(AIKnowledge knowledge) => default;
		public static bool IsCombatFollowMoveValid(AIKnowledge knowledge) => default;

		public static bool IsFreeSkillValid(AIKnowledge knowledge)
		{
			return knowledge.skillKnowledge.SkillsFree.AvailableSkills.Count > 0;
		}
		public static bool IsFollowScriptedPathValid(AIKnowledge knowledge) => default;
		public static bool IsSpacialProbeValid(AIKnowledge knowledge) => default;
		public static bool IsWanderValid(AIKnowledge knowledge)
		{
			if (!knowledge.moveKnowledge.canMove) return false;
			if (knowledge.wanderTactic.config==null || !knowledge.wanderTactic.config.Enable)
				return false;
			if (knowledge.moveControlState.WanderInfo.nextAvailableTick > GameTimerManager.Instance.GetTimeNow())
				return false;
			return true;
		}

		public static bool IsIdleValid(AIKnowledge knowledge) => default;
		public static bool IsCombatValid(AIKnowledge knowledge) => default;

		public static bool IsCombatBuddySkillValid(AIKnowledge knowledge)
		{
			return knowledge.skillKnowledge.SkillsCombatBuddy.AvailableSkills.Count > 0;
		}

		public static bool IsCombatSkillExecuteValid(AIKnowledge knowledge)
		{
			return knowledge.actionControlState.status == SkillStatus.Prepared;
		}

		public static bool IsCombatSkillPrepareValid(AIKnowledge knowledge)
		{
			return knowledge.skillKnowledge.SkillsCombat.AvailableSkills.Count > 0;
		}
		public static bool IsCombatFixedMoveValid(AIKnowledge knowledge) => default;
		public static bool IsCombatMeleeChargeValid(AIKnowledge knowledge)
		{
			if (!knowledge.moveKnowledge.canMove) return false;
			knowledge.meleeChargeTactic.SwitchSetting(knowledge.poseID);
			float meleeChargeStartDistanceMin = knowledge.meleeChargeTactic.data.startDistanceMin;
			float meleeChargeStartDistanceMax = knowledge.meleeChargeTactic.data.startDistanceMax;
			
			if (knowledge.meleeChargeTactic.config==null||!knowledge.meleeChargeTactic.config.Enable)
				return false;
			if (!knowledge.meleeChargeTactic.NerveCheck(knowledge))
				return false;
			if (knowledge.targetKnowledge.targetDistance > meleeChargeStartDistanceMax)
				return false;
			if (knowledge.targetKnowledge.targetDistance < meleeChargeStartDistanceMin)
				return false;
			return true;
		}
		public static bool IsCombatFacingMoveValid(AIKnowledge knowledge)
		{
			if (!knowledge.moveKnowledge.canMove) return false;
			if (knowledge.facingMoveTactic.config==null||!knowledge.facingMoveTactic.config.Enable)
				return false;
			if (!knowledge.facingMoveTactic.NerveCheck(knowledge))
				return false;
			return true;
		}
		public static bool IsCombatSurroundValid(AIKnowledge knowledge) => default;
		public static bool IsCombatFindBackValid(AIKnowledge knowledge) => default;
		public static bool IsCombatCrabMoveValid(AIKnowledge knowledge) => default;
		public static bool IsCombatSpacialChaseValid(AIKnowledge knowledge) => default;
		public static bool IsCombatSpacialAdjustValid(AIKnowledge knowledge) => default;
		public static bool IsCombatIdleValid(AIKnowledge knowledge) => default;
		public static bool IsScriptedMoveToValid(AIKnowledge knowledge) => default;
		public static bool IsLandingValid(AIKnowledge knowledge) => default;
		public static bool IsExtractionValid(AIKnowledge knowledge) => default;
		public static bool IsFleeValid(AIKnowledge knowledge)
		{
			if (!knowledge.moveKnowledge.canMove) return false;
			knowledge.fleeTactic.SwitchSetting(knowledge.poseID);
			float triggerDistance = knowledge.fleeTactic.data.triggerDistance;
			if (knowledge.fleeTactic.config==null||!knowledge.fleeTactic.config.Enable)
				return false;
			if (!knowledge.fleeTactic.NerveCheck(knowledge))
				return false;
			if (knowledge.moveControlState.FleeInfo.nextAvailableTick > GameTimerManager.Instance.GetTimeNow())
				return false;
			if (knowledge.targetKnowledge.targetDistance > triggerDistance)
				return false;
			return true;
		}
		public static bool IsBirdCirclingValid(AIKnowledge knowledge) => default;
		public static bool IsBrownianMotionValid(AIKnowledge knowledge) => default;
		public static bool IsAutoPlayerFollowTargetValid(AIKnowledge knowledge) => default;
		public static bool IsAutoPlayerCombatSkillExecuteValid(AIKnowledge knowledge) => default;
		public static bool IsAutoPlayerCombatSkillPrepareValid(AIKnowledge knowledge) => default;
	}
}