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
			var conf = ConfigAIDecisionTreeCategory.Instance.Get(knowledge.decisionArchetype);
			if (conf != null)
			{
				if (knowledge.combatComponent != null && knowledge.combatComponent.isInCombat)
				{
					if (conf.CombatNode != null)
						Handler(knowledge, decision, conf.CombatNode);
				}
				else
				{
					if (conf.Node != null)
						Handler(knowledge, decision, conf.Node);
				}
			}
		}

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
					Log.Error("AI行为树未找到指定方法" + conditionNode.Condition);
				}
			}
			else
			{
				Log.Error("AI行为树未配置节点");
			}
		}

		#endregion
	}
}