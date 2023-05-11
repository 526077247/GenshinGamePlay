using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

namespace TaoTie
{
    /// <summary>
    /// 去掉Animator中的连线
    /// </summary>
    public static class AnimatorControllerExporter
    {
        public static readonly Dictionary<string, int> animatorControllerParameterUsed = new();
        public static readonly Dictionary<string, int> animatorControllerParameterUsedByTransition = new();

        public static void Export(string srcPath, string savePath, bool publish)
        {
            var srcController = AssetDatabase.LoadAssetAtPath<AnimatorController>(srcPath);
            if (srcController != null)
            {
                animatorControllerParameterUsed.Clear();
                animatorControllerParameterUsedByTransition.Clear();
                File.Copy(srcPath, savePath, true);
                AssetDatabase.Refresh();
                AnimatorController newAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(savePath);
                if (newAnimator == null)
                {
                    return;
                }
                for (int i = 0; i < newAnimator.layers.Length; i++)
                {
                    var baseLayer = newAnimator.layers[i];
                    if (baseLayer.syncedLayerIndex >= 0)
                        continue;
                    ClearAnimatorStateMachine(baseLayer.stateMachine);
                }

                List<AnimatorControllerParameter> paras = new List<AnimatorControllerParameter>();
                for (int i = 0; i < newAnimator.parameters.Length; i++)
                {
                    if (animatorControllerParameterUsed.ContainsKey(newAnimator.parameters[i].name))
                    {
                        paras.Add(newAnimator.parameters[i]);
                    }
                    else if (!animatorControllerParameterUsedByTransition.ContainsKey(newAnimator.parameters[i].name))
                    {
                        Debug.Log("未被使用的参数" + newAnimator.parameters[i].name);
                    }
                }
                newAnimator.parameters = paras.ToArray();

                newAnimator.name = srcController.name;
                if (!publish)
                {
                    File.Delete(savePath);
                    AssetDatabase.Refresh();
                    return;
                }
                AssetDatabase.SaveAssetIfDirty(newAnimator);
                StringBuilder builder = new StringBuilder();
                var lines = File.ReadAllLines(savePath);

                //Unity的BUG删不干净StateMachine连到Exit的线
                bool flag = false;
                bool flag2 = false;
                int startPos = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!flag)
                    {
                        if (!lines[i].Contains("AnimatorTransition"))
                        {
                            if (!flag2)
                                builder.AppendLine(lines[i - 1]);
                        }
                        else
                        {
                            flag = true;
                            startPos = i;
                        }
                    }
                    else
                    {
                        if (lines[i].Contains("m_IsExit: 0"))//非连到Exit的线
                        {
                            i = startPos;
                            builder.AppendLine(lines[i - 1]);
                            flag = false;
                        }
                        if (lines[i].Contains("--- !"))
                        {
                            flag = false;
                        }
                    }

                    if (!flag2)
                    {
                        if (lines[i].Contains("m_StateMachineTransitions:"))
                            flag2 = true;
                    }
                    else
                    {
                        if (lines[i].Contains("m_StateMachineBehaviours:"))
                        {
                            builder.AppendLine("  m_StateMachineTransitions: {}");
                            flag2 = false;
                        }
                    }
                }
                builder.AppendLine(lines[lines.Length - 1]);
                File.WriteAllText(savePath, builder.ToString());
                Debug.Log(savePath);
                AssetDatabase.Refresh();
            }
        }

        private static void ClearAnimatorStateMachine(AnimatorStateMachine baseStateMachine)
        {
            for (int i = 0; i < baseStateMachine.states.Length; i++)
            {
                ClearAnimatorState(baseStateMachine.states[i].state);
            }
            for (int i = 0; i < baseStateMachine.stateMachines.Length; i++)
            {
                ClearChildAnimatorStateMachine(baseStateMachine.stateMachines[i]);
            }

            for (int i = baseStateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                var transition = baseStateMachine.anyStateTransitions[i];
                for (int j = 0; j < transition.conditions.Length; j++)
                {
                    RecordTransitionUsageParam(transition.conditions[j].parameter);
                }
            }
            baseStateMachine.anyStateTransitions = Array.Empty<AnimatorStateTransition>();
            for (int i = baseStateMachine.entryTransitions.Length - 1; i >= 0; i--)
            {
                var transition = baseStateMachine.entryTransitions[i];
                for (int j = 0; j < transition.conditions.Length; j++)
                {
                    RecordTransitionUsageParam(transition.conditions[j].parameter);
                }
            }
            baseStateMachine.entryTransitions = Array.Empty<AnimatorTransition>();
        }

        private static void ClearChildAnimatorStateMachine(ChildAnimatorStateMachine baseStateMachine)
        {
            ClearAnimatorStateMachine(baseStateMachine.stateMachine);
        }

        private static void ClearAnimatorState(AnimatorState baseState)
        {
            baseState.behaviours = Array.Empty<StateMachineBehaviour>();
            for (int i = baseState.transitions.Length - 1; i >= 0; i--)
            {
                var transition = baseState.transitions[i];
                for (int j = 0; j < transition.conditions.Length; j++)
                {
                    RecordTransitionUsageParam(transition.conditions[j].parameter);
                }
            }
            baseState.transitions = Array.Empty<AnimatorStateTransition>();
            RecordUsageParam(baseState.mirrorParameter);
            RecordUsageParam(baseState.speedParameter);
            RecordUsageParam(baseState.timeParameter);
            RecordUsageParam(baseState.cycleOffsetParameter);
            if (baseState.motion is BlendTree tree)
            {
                ClearBlendTree(tree);
            }

        }
        private static void ClearBlendTree(BlendTree tree)
        {
            for (int i = 0; i < tree.children.Length; i++)
            {
                if (tree.children[i].motion is BlendTree childtree)
                {
                    ClearBlendTree(childtree);
                }
            }

            RecordUsageParam(tree.blendParameter);
            RecordUsageParam(tree.blendParameterY);
        }

        /// <summary>
        /// 非Transition用到的参数
        /// </summary>
        /// <param name="name"></param>
        private static void RecordUsageParam(string name)
        {
            if (animatorControllerParameterUsed.ContainsKey(name))
            {
                animatorControllerParameterUsed[name]++;
            }
            else
            {
                animatorControllerParameterUsed[name] = 1;
            }
        }
        /// <summary>
        /// Transition用到的参数
        /// </summary>
        /// <param name="name"></param>
        private static void RecordTransitionUsageParam(string name)
        {
            if (animatorControllerParameterUsedByTransition.ContainsKey(name))
            {
                animatorControllerParameterUsedByTransition[name]++;
            }
            else
            {
                animatorControllerParameterUsedByTransition[name] = 1;
            }
        }
    }
}