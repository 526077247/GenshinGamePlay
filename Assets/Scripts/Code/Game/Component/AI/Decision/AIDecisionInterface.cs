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
			return knowledge.SkillKnowledge.SkillsOnAware.AvailableSkills.Count > 0;
		}

		public static bool IsOnAlertValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0;
		}

		public static bool IsOnNerveValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsOnNerve.AvailableSkills.Count > 0;
		}
		public static bool IsReturnToBornPosValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
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
			return knowledge.SkillKnowledge.SkillsFree.AvailableSkills.Count > 0;
		}
		public static bool IsFollowScriptedPathValid(AIKnowledge knowledge) => default;
		public static bool IsSpacialProbeValid(AIKnowledge knowledge) => default;
		public static bool IsWanderValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.WanderTactic.Config==null || !knowledge.WanderTactic.Config.Enable)
				return false;
			if (knowledge.MoveControlState.WanderInfo.NextAvailableTick > GameTimerManager.Instance.GetTimeNow())
				return false;
			return true;
		}

		public static bool IsIdleValid(AIKnowledge knowledge) => default;
		public static bool IsCombatValid(AIKnowledge knowledge) => default;

		public static bool IsCombatBuddySkillValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills.Count > 0;
		}

		public static bool IsCombatSkillExecuteValid(AIKnowledge knowledge)
		{
			return knowledge.ActionControlState.Status == SkillStatus.Prepared;
		}

		public static bool IsCombatSkillPrepareValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0;
		}
		public static bool IsCombatFixedMoveValid(AIKnowledge knowledge) => default;
		public static bool IsCombatMeleeChargeValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.MoveControlState.MeleeCharge.Status == MeleeChargeInfo.ChargeStatus.Charging) return true;
			knowledge.MeleeChargeTactic.SwitchSetting(knowledge.PoseID);
			float meleeChargeStartDistanceMin = knowledge.MeleeChargeTactic.Data.StartDistanceMin;
			float meleeChargeStartDistanceMax = knowledge.MeleeChargeTactic.Data.StartDistanceMax;
			
			if (knowledge.MeleeChargeTactic.Config==null||!knowledge.MeleeChargeTactic.Config.Enable)
				return false;
			if (!knowledge.MeleeChargeTactic.NerveCheck(knowledge))
				return false;
			if (knowledge.TargetKnowledge.TargetDistance > meleeChargeStartDistanceMax)
				return false;
			if (knowledge.TargetKnowledge.TargetDistance < meleeChargeStartDistanceMin)
				return false;
			return true;
		}
		public static bool IsCombatFacingMoveValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.FacingMoveTactic.Config==null||!knowledge.FacingMoveTactic.Config.Enable)
				return false;
			if (!knowledge.FacingMoveTactic.NerveCheck(knowledge))
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
			if (!knowledge.MoveKnowledge.CanMove) return false;
			knowledge.FleeTactic.SwitchSetting(knowledge.PoseID);
			float triggerDistance = knowledge.FleeTactic.Data.TriggerDistance;
			if (knowledge.FleeTactic.Config==null||!knowledge.FleeTactic.Config.Enable)
				return false;
			if (!knowledge.FleeTactic.NerveCheck(knowledge))
				return false;
			if (knowledge.MoveControlState.FleeInfo.NextAvailableTick > GameTimerManager.Instance.GetTimeNow())
				return false;
			if (knowledge.TargetKnowledge.TargetDistance > triggerDistance)
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