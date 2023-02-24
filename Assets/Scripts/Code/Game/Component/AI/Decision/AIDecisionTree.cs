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

		#region Tree

		private static void Handler(AIKnowledge knowledge, AIDecision decision, DecisionNode tree)
		{
			if (tree is DecisionActionNode actionNode)
			{
				decision.act = actionNode.Act;
				decision.move = actionNode.Move;
				decision.tactic = actionNode.Tactic;
			}
			else if (tree is DecisionConditionNode conditionNode)
			{
				if (AIDecisionInterface.Methods.TryGetValue(conditionNode.Condition, out var func))
				{
					if (func(knowledge))
					{
						Handler(knowledge, decision, conditionNode.True);
					}
					else
					{
						Handler(knowledge, decision, conditionNode.False);
					}
				}
				else
				{
					Log.Error("AI行为树未找到指定方法"+conditionNode.Condition);
				}
			}
		}

		#endregion
	}
}