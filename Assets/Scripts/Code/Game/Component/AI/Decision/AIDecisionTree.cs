namespace TaoTie
{
	/// <summary>
	/// 决策行为树
	/// </summary>
	public static class AIDecisionTree
	{
		/// <summary>
		/// 决策
		/// </summary>
		/// <param name="knowledge"></param>
		/// <param name="decision"></param>
		public static void Think(AIKnowledge knowledge, AIDecision decision)
		{
			switch (knowledge.decisionArchetype)
			{
				case DecisionArchetype.Animal:
					RootAnimal(knowledge, decision);
					break;
				case DecisionArchetype.Cicin:
					RootCicin(knowledge, decision);
					break;
				case DecisionArchetype.Dahaka:
					RootDahaka(knowledge, decision);
					break;
				case DecisionArchetype.Animal_Homeworld:
					RootAnimal_Homeworld(knowledge, decision);
					break;
				case DecisionArchetype.General:
					RootGeneral(knowledge, decision);
					break;
			}
		}

		#region Animal

		private static void RootAnimal(AIKnowledge knowledge, AIDecision decision)
		{
		}

		private static void CombatAnimal(AIKnowledge knowledge, AIDecision decision)
		{
		}

		private static void RootAnimal_Homeworld(AIKnowledge knowledge, AIDecision decision)
		{
		}

		private static void CombatAnimal_Homeworld(AIKnowledge knowledge, AIDecision decision)
		{
		}

		#endregion

		#region Cicin

		private static void RootCicin(AIKnowledge knowledge, AIDecision decision)
		{
			if (AIDecisionInterface.IsReturnToBornPosValid(knowledge))
			{
				AIDecisionFactory.ReturnToBornPos(knowledge, decision);
			}
			else if (knowledge.threatLevel >= ThreatLevel.Alert)
			{
				CombatCicin(knowledge, decision);
			}
			else
			{
				if (AIDecisionInterface.IsWanderValid(knowledge))
				{
					AIDecisionFactory.Wander(knowledge, decision);
				}
				else
				{
					AIDecisionFactory.Idle(knowledge, decision);
				}
			}
		}

		private static void CombatCicin(AIKnowledge knowledge, AIDecision decision)
		{
		}


		#endregion

		#region Dahaka

		private static void RootDahaka(AIKnowledge knowledge, AIDecision decision)
		{
		}

		private static void CombatDahaka(AIKnowledge knowledge, AIDecision decision)
		{
		}


		#endregion

		#region General

		private static void RootGeneral(AIKnowledge knowledge, AIDecision decision)
		{
		}

		private static void CombatGeneral(AIKnowledge knowledge, AIDecision decision)
		{
		}
		

		#endregion
		

	}
}