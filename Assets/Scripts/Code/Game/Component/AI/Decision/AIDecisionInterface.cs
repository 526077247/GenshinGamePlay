using System;
using System.Collections.Generic;
using Obfuz;
using Sirenix.OdinInspector;

namespace TaoTie
{
	[ObfuzIgnore]
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

		/// <summary> 感知阶段技能触发条件。对应 AITactic.OnAware / ActDecision.OnAware。
		/// 当威胁等级刚达到 Aware(警惕)时，检查是否有 OnAware 类型的技能可用。 </summary>
		[ObfuzIgnore]
		public static bool IsOnAwareValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsOnAware.AvailableSkills.Count > 0;
		}

		/// <summary> 警报阶段技能触发条件。对应 AITactic.OnAlert / ActDecision.OnAlert。
		/// 当威胁等级达到 Alert(警报)进入战斗时，检查是否有 OnAlert 类型的技能可用。 </summary>
		[ObfuzIgnore]
		public static bool IsOnAlertValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsOnAlert.AvailableSkills.Count > 0;
		}

		/// <summary> 狂暴阶段技能触发条件。对应 AITactic.OnNerve / ActDecision.OnNerve。
		/// 当怪物进入狂暴状态时，检查是否有 OnNerve 类型的技能可用。 </summary>
		[ObfuzIgnore]
		public static bool IsOnNerveValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsOnNerve.AvailableSkills.Count > 0;
		}

		/// <summary> 返回出生点条件。对应 AITactic.ReturnToBornPos / MoveDecision.ReturnToBorn。
		/// 失去目标后返回出生位置，需可移动。 </summary>
		[ObfuzIgnore]
		public static bool IsReturnToBornPosValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			// if (!knowledge.returnToBornKnowledge.config.enable)
			// 	return false;
			// if (!knowledge.isReturnToBorn)
			// 	return false;
			return true;
		}

		/// <summary> 调查条件。对应 AITactic.Investigate / MoveDecision.Investigate。
		/// 目标已丢失但存在最后已知位置(TargetLKP)，前往该位置搜索。 </summary>
		[ObfuzIgnore]
		public static bool IsInvestigateValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			return knowledge.TargetKnowledge.TargetLKP != null
				&& knowledge.TargetKnowledge.TargetEntity == null;
		}

		/// <summary> 行动点技能条件。对应 AITactic.ReactActionPoint / ActDecision.ActionPointSkill。
		/// 检查是否有 ActionPoint(对地点)类型的技能可用。 </summary>
		[ObfuzIgnore]
		public static bool IsReactActionPointValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsActionPoint.AvailableSkills.Count > 0;
		}

		/// <summary> 巡逻跟随条件。对应 AITactic.PatrolFollow / MoveDecision.PatrolFollow。
		/// 非战斗状态下(威胁等级低于Alert)沿巡逻路线移动。 </summary>
		[ObfuzIgnore]
		public static bool IsPatrolFollowValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			return knowledge.ThreatLevel < ThreatLevel.Alert;
		}

		/// <summary> 战斗跟随移动条件。对应 AITactic.CombatFollowMove / MoveDecision.CombatFollowMove。
		/// 目标距离过远(>5m)时向目标靠近。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatFollowMoveValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.TargetKnowledge.TargetDistance > 5f;
		}

		/// <summary> 自由技能条件。对应 AITactic.FreeSkill / ActDecision.FreeSkill。
		/// 非战斗状态下检查是否有 Free 类型技能可用。 </summary>
		[ObfuzIgnore]
		public static bool IsFreeSkillValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsFree.AvailableSkills.Count > 0;
		}

		/// <summary> 空间探查条件。对应 AITactic.SpacialProbe / MoveDecision.SpacialProbe。
		/// 目标在空中时触发空中探查移动。 </summary>
		[ObfuzIgnore]
		public static bool IsSpacialProbeValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			return knowledge.TargetKnowledge.TargetInAir;
		}

		/// <summary> 漫游条件。对应 AITactic.Wander / MoveDecision.Wander。
		/// 非战斗状态下在出生点附近随机漫游，需配置启用且不在CD中。 </summary>
		[ObfuzIgnore]
		public static bool IsWanderValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.WanderTactic.Config==null || !knowledge.WanderTactic.Config.Enable)
				return false;
			if (knowledge.MoveControlState.WanderInfo.Status == WanderInfo.WanderStatus.Wandering) return true;

			if (knowledge.MoveControlState.WanderInfo.NextAvailableTick > GameTimerManager.Instance.GetTimeNow())
				return false;
			return true;
		}

		/// <summary> 战斗状态判断。对应 AITactic.CombatSkill 系列。
		/// 优先检查 CombatComponent.IsInCombat，无则回退到威胁等级>=Alert。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatValid(AIKnowledge knowledge)
		{
			if (knowledge.CombatComponent != null) return knowledge.CombatComponent.IsInCombat;
			return knowledge.ThreatLevel >= ThreatLevel.Alert;
		}

		/// <summary> 伙伴技能条件。对应 AITactic.BuddySkill / ActDecision.BuddySkill。
		/// 检查是否有 CombatBuddy(对伙伴使用)类型技能可用。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatBuddySkillValid(AIKnowledge knowledge)
		{
			return knowledge.SkillKnowledge.SkillsCombatBuddy.AvailableSkills.Count > 0;
		}

		/// <summary> 战斗技能执行条件。对应 AITactic.CombatSkill / ActDecision.CombatSkill。
		/// 技能已准备完毕(IsPrepared)，可以立即释放。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatSkillExecuteValid(AIKnowledge knowledge)
		{
			return knowledge.ActionControlState.IsPrepared;
		}

		/// <summary> 战斗技能准备条件。对应 AITactic.CombatSkillPrepare / ActDecision.CombatSkillPrepare。
		/// 正在准备中或处于空闲状态且有可用战斗技能时触发。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatSkillPrepareValid(AIKnowledge knowledge)
		{
			if (knowledge.ActionControlState.Status == SkillStatus.Preparing) return true;
			if (knowledge.ActionControlState.Status != SkillStatus.Inactive) return false;
			return knowledge.SkillKnowledge.SkillsCombat.AvailableSkills.Count > 0;
		}

		/// <summary> 战斗固定移动条件。对应 AITactic.CombatFixedMove / MoveDecision.CombatFixedMove。
		/// 战斗中(威胁>=Alert)朝目标做固定路线移动。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatFixedMoveValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.ThreatLevel >= ThreatLevel.Alert;
		}

		/// <summary> 近战冲锋条件。对应 AITactic.MeleeCharge / MoveDecision.MeleeCharge。
		/// 目标距离在配置的[StartDistanceMin, StartDistanceMax]范围内时冲锋，需配置启用且Pose匹配。
		/// 正在冲锋中则持续有效。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatMeleeChargeValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.MeleeChargeTactic.Config==null||!knowledge.MeleeChargeTactic.Config.Enable)
				return false;
			
			if (knowledge.MoveControlState.MeleeCharge.Status == MeleeChargeInfo.ChargeStatus.Charging) return true;
			float meleeChargeStartDistanceMin = knowledge.MeleeChargeTactic.Data.StartDistanceMin;
			float meleeChargeStartDistanceMax = knowledge.MeleeChargeTactic.Data.StartDistanceMax;
			
			if (!knowledge.MeleeChargeTactic.NerveCheck(knowledge))
				return false;
			if (knowledge.TargetKnowledge.TargetDistance > meleeChargeStartDistanceMax)
				return false;
			if (knowledge.TargetKnowledge.TargetDistance < meleeChargeStartDistanceMin)
				return false;
			return true;
		}

		/// <summary> 面向移动条件。对应 AITactic.FacingMove / MoveDecision.FacingMove。
		/// 战斗中围绕目标做前后左右平移(斯特拉菲)，需配置启用且Pose匹配。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatFacingMoveValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.FacingMoveTactic.Config==null||!knowledge.FacingMoveTactic.Config.Enable)
				return false;
			if (!knowledge.FacingMoveTactic.NerveCheck(knowledge))
				return false;
			return true;
		}

		/// <summary> 环绕对峙条件。对应 AITactic.Surround / MoveDecision.Surround。
		/// 目标在中距离(2~8m)时围绕目标做环绕移动。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatSurroundValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			float dist = knowledge.TargetKnowledge.TargetDistance;
			return dist > 2f && dist < 8f;
		}

		/// <summary> 背后寻找条件。对应 AITactic.FindBack / MoveDecision.FindBack。
		/// AI自身在目标背后时触发绕背攻击。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatFindBackValid(AIKnowledge knowledge)
		{
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.TargetKnowledge.IsSelfAtTargetBack;
		}

		/// <summary> 螃蟹步条件。对应 AITactic.CrabMove / MoveDecision.CrabMove。
		/// 目标近距离(<4m)时做侧向横移闪避。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatCrabMoveValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.TargetKnowledge.TargetDistance < 4f;
		}

		/// <summary> 空间追击条件。对应 AITactic.SpacialChase / MoveDecision.SpacialChase。
		/// 目标在空中时触发空中追击移动。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatSpacialChaseValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.TargetKnowledge.TargetInAir;
		}

		/// <summary> 空间调整条件。对应 AITactic.SpacialAdjust / MoveDecision.SpacialAdjust。
		/// 与目标之间无视线(被障碍物遮挡)时调整站位。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatSpacialAdjustValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return !knowledge.TargetKnowledge.HasLineOfSight;
		}

		/// <summary> 战斗待机条件。对应 AITactic.CombatIdle。
		/// 有目标且威胁等级>=Alert时在原地待命。 </summary>
		[ObfuzIgnore]
		public static bool IsCombatIdleValid(AIKnowledge knowledge)
		{
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.ThreatLevel >= ThreatLevel.Alert;
		}

		/// <summary> 脚本移动条件。对应 AITactic.ScriptedMoveTo / MoveDecision.ScriptedMoveTo。
		/// 目标类型为 PointTarget(位置目标)时移动到指定坐标。 </summary>
		[ObfuzIgnore]
		public static bool IsScriptedMoveToValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			return knowledge.TargetKnowledge.TargetType == AITargetType.PointTarget;
		}

		/// <summary> 降落条件。对应 AITactic.Landing / MoveDecision.Landing。
		/// 当前高度高于出生点0.5m时触发降落行为。 </summary>
		[ObfuzIgnore]
		public static bool IsLandingValid(AIKnowledge knowledge)
		{
			return knowledge.CurrentPos.y > knowledge.BornPos.y + 0.5f;
		}

		/// <summary> 撤退条件。对应 AITactic.Extraction / MoveDecision.Extraction。
		/// HP低于30%时触发撤退行为。 </summary>
		[ObfuzIgnore]
		public static bool IsExtractionValid(AIKnowledge knowledge)
		{
			if (knowledge.Numeric == null) return false;
			float hp = knowledge.Numeric.GetAsFloat(NumericType.Hp);
			float maxHp = knowledge.Numeric.GetAsFloat(NumericType.MaxHp);
			if (maxHp <= 0) return false;
			return hp / maxHp < 0.3f;
		}

		/// <summary> 逃跑条件。对应 AITactic.Flee / MoveDecision.Flee。
		/// 目标距离在触发距离内、配置启用、不在CD中时远离目标逃跑。 </summary>
		[ObfuzIgnore]
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

		/// <summary> 飞鸟盘旋条件。对应 AITactic.BirdCircling / MoveDecision.BirdCircling。
		/// 目标距离较远(>5m)时做空中盘旋移动，适用于飞行类怪物。 </summary>
		[ObfuzIgnore]
		public static bool IsBirdCirclingValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			if (knowledge.TargetKnowledge.TargetEntity == null) return false;
			return knowledge.TargetKnowledge.TargetDistance > 5f;
		}

		/// <summary> 自动玩家跟随条件。对应 AITactic.AutoPlayerFollowTarget / MoveDecision.AutoPlayerFollowTarget。
		/// 有目标实体时自动玩家跟随目标移动。 </summary>
		[ObfuzIgnore]
		public static bool IsAutoPlayerFollowTargetValid(AIKnowledge knowledge)
		{
			if (!knowledge.MoveKnowledge.CanMove) return false;
			return knowledge.TargetKnowledge.TargetEntity != null;
		}
	}
}
