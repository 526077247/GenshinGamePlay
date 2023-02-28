using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TaoTie
{
    class Transition
    {
        public string toState;
        public List<ConfigCondition> conditionList = new List<ConfigCondition>();

        public void CombineComditions(List<ConfigCondition> other)
        {
            conditionList.AddRange(from condition in other select condition.Copy());
        }

        public void CombineConditions(Transition other)
        {
            CombineComditions(other.conditionList);
        }
    }

    public class FsmExporter
    {
        private AnimatorController _controller = null;
        private string _fsmActionsPath = null;
        private bool _publish = false;
        private Dictionary<string, ConfigFsmTimeline> _fsmTimelineDict;
        private Dictionary<string, ConfigParam> _paramDict;
        private bool _hasError = false;
        private AnimatorStateMachine _baseSm = null;
        private string _defaultStateName = null;

        public FsmExporter(AnimatorController controller,  string actionPath,bool publish)
        {
            _publish = publish;
            _controller = controller;
            _fsmActionsPath = actionPath;
        }

        public void Generate(string _controllerConfigName)
        {
            bool isNew = false;
            string controllerPath = AssetDatabase.GetAssetPath(_controller);
            string controllerSavePath = controllerPath.Replace(UtilityEditor.EditDirName,UtilityEditor.PublishDirName);
            string configSavePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(controllerPath)), _controllerConfigName);

            _hasError = false;
            ExportController(controllerPath, controllerSavePath);
            ExportParam();

            LoadFsmTimeline();
            List<ConfigFsm> fsmList = new List<ConfigFsm>();
            for (int i = 0; i < _controller.layers.Length; ++i)
            {
                if (_controller.layers[i].syncedLayerIndex >= 0)
                    continue;
                var cfgFsm = ExportLayer(i);
                fsmList.Add(cfgFsm);
            }

            ConfigFsmController newController = new ConfigFsmController();

            newController.fsmConfigs = fsmList.ToArray();
            newController.paramDict = _paramDict;

            if (!_hasError)
            {
                File.WriteAllText(configSavePath, JsonHelper.ToJson(newController));
                AssetDatabase.Refresh();
                Debug.LogFormat("导出成功! {0}", configSavePath);
            }
            else
            {
                Debug.LogFormat("导出失败!");
            }
        }

        private void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        #region Fsm Controller
        private void ExportController(string srcPath, string savePath)
        {
            string dir = Path.GetDirectoryName(savePath);
            EnsureDir(dir);
            AnimatorControllerExporter.Export(srcPath, savePath, _publish);
        }
        #endregion

        #region Fsm Timeline
        private void LoadFsmTimeline()
        {
            _fsmTimelineDict = new Dictionary<string, ConfigFsmTimeline>();
            LoadFsmTimelineInPath(_fsmActionsPath);
        }

        private void LoadFsmTimelineInPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
                return;

            FileInfo[] fileInfos = dirInfo.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);
            foreach (FileInfo fileInfo in fileInfos)
            {
                ConfigFsmTimeline timeline = TimelineSerializer.GetTimeline(Path.Combine(path, fileInfo.Name));
                _fsmTimelineDict.Add(fileInfo.Name.Split('.')[0], timeline);
            }
        }
        #endregion

        #region Fsm Param
        private Dictionary<string, ConfigParam> ExportParam()
        {
            _paramDict = new Dictionary<string, ConfigParam>();
            foreach (var param in _controller.parameters)
            {
                var paramCfg = GenerateParameters(param);
                if (paramCfg != null)
                {
                    _paramDict[paramCfg.key] = paramCfg;
                }
            }

            return _paramDict;
        }

        private ConfigParam GenerateParameters(AnimatorControllerParameter param)
        {
            bool animUse = AnimatorControllerExporter.animatorControllerParameterUsed.ContainsKey(param.name);

            switch (param.type)
            {
                case AnimatorControllerParameterType.Bool:
                {
                        return new ConfigParamBool(param.name, param.defaultBool, animUse);
                    }
                case AnimatorControllerParameterType.Trigger:
                    {
                        return new ConfigParamTrigger(param.name, param.defaultBool, animUse);
                    }
                case AnimatorControllerParameterType.Int:
                    {
                        return new ConfigParamInt(param.name, param.defaultInt, animUse);
                    }
                case AnimatorControllerParameterType.Float:
                    {
                        return new ConfigParamFloat(param.name, param.defaultFloat, animUse);
                    }
                default:
                    break;
            }
            return null;
        }

        private AnimatorControllerParameter GetParameter(string name)
        {
            return Array.Find(_controller.parameters, a => a.name == name);
        }
        #endregion

        private ConfigFsm ExportLayer(int layerIndex)
        {
            AnimatorControllerLayer layer = _controller.layers[layerIndex];
            List<ConfigFsmState> stateList = new List<ConfigFsmState>();
            List<ConfigTransition> anyStateTransitionList = new List<ConfigTransition>();

            List<AnimatorStateMachine> parentStack = new List<AnimatorStateMachine>();
            _baseSm = layer.stateMachine;
            _defaultStateName = layer.stateMachine.defaultState.name;
            ExportStateMachine(parentStack, layer.stateMachine, ref stateList, ref anyStateTransitionList);

            ConfigFsm cfgFsm = new ConfigFsm(layer.name, layerIndex);
            cfgFsm.entry = _defaultStateName;
            cfgFsm.SetStates(stateList);
            cfgFsm.SetAnyStateTransitions(anyStateTransitionList.ToArray());

            return cfgFsm;
        }

        private void ExportStateMachine(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm, ref List<ConfigFsmState> stateList, ref List<ConfigTransition> anyStateTransitionList)
        {
            foreach (var state in sm.states)
            {
                stateList.Add(ExportState(parentStack, sm, state.state));
            }
            ExportAnyStateTransitions(parentStack, sm, ref anyStateTransitionList);
            parentStack.Add(sm);
            foreach (var state in sm.stateMachines)
            {
                ExportStateMachine(parentStack, state.stateMachine, ref stateList, ref anyStateTransitionList);
            }
            parentStack.RemoveAt(parentStack.Count - 1);
        }

        private void ExportAnyStateTransitions(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm, ref List<ConfigTransition> anyStateTransitionList)
        {
            AnimatorStateTransition[] transitions = sm.anyStateTransitions;
            if (transitions != null)
            {
                for (int i = 0; i < transitions.Length; ++i)
                {
                    AnimatorStateTransition transition = transitions[i];
                    if (transition.conditions == null || transition.conditions.Length == 0)
                    {
                        // 没有条件的AnyStateTransition不生效
                        continue;
                    }
                    ExportStateTransition(parentStack, sm, null, transition, true, ref anyStateTransitionList);
                }
            }
        }

        private ConfigFsmState ExportState(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm, AnimatorState state)
        {
            List<ConfigTransition> transList = new List<ConfigTransition>();
            foreach (var tran in state.transitions)
            {
                ExportStateTransition(parentStack, sm, state, tran, false, ref transList);
            }

            ConfigFsmTimeline timeline;
            _fsmTimelineDict.TryGetValue(state.name, out timeline);

            float stateDuration = 1f;
            var stateLoop = false;
            string mirrorParam = null;
            if (state.motion != null)
            {
                stateLoop = state.motion.isLooping;
                stateDuration = state.motion.averageDuration;
                if (state.motion is BlendTree bt)
                {
                    if (bt.blendParameter != null && bt.blendParameter.StartsWith("@"))
                        mirrorParam = bt.blendParameter;
                    else if (bt.blendParameterY != null && bt.blendParameterY.StartsWith("@"))
                        mirrorParam = bt.blendParameterY;
                }
            }
            else if (timeline != null)
            {
                stateDuration = timeline.length;
            }

            ConfigFsmState ret = new ConfigFsmState(state.name, stateDuration, stateLoop, mirrorParam);
            ret.transitions = transList.ToArray();
            ret.timeline = timeline;
            if (state.behaviours.Length > 0)
            {
                for (int i = 0; i < state.behaviours.Length; i++)
                {
                    if (state.behaviours[i] is AnimatorParam param)
                    {
                        ret.data = param.Data;
                    }
                }
            }
            return ret;
        }

        private void ExportStateTransition(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm, AnimatorState state, AnimatorStateTransition tran, bool isAnyStateTransition, ref List<ConfigTransition> ret)
        {
            List<ConfigCondition> conditionList = new List<ConfigCondition>();
            foreach (var item in tran.conditions)
            {
                conditionList.Add(ExportCondition(item));
            }
            if (tran.hasExitTime)
            {
                conditionList.Add(new ConfigConditionByStateTime(tran.exitTime, true, CompareMode.GEqual));
            }

            // do export transition without conditions
            if (conditionList.Count == 0)
                return;

            float fadeDur = 0.25f;
            if (tran.hasFixedDuration)
            {
                fadeDur = tran.duration;
            }
            else
            {
                Debug.LogWarning($"[FsmExporter:ExportTransition] transition {tran.name} can not use fade duration in normalized time");
            }

            float offset = tran.offset;

            if (tran.destinationState != null)
            {
                if (tran.destinationState.motion != null)
                {
                    offset *= tran.destinationState.motion.averageDuration;
                }
                bool canTransitionToSelf = isAnyStateTransition ? tran.canTransitionToSelf : state?.name == tran.destinationState.name;
                ret.Add(new ConfigTransition(state?.name, tran.destinationState.name, conditionList.ToArray(), fadeDur, offset, canTransitionToSelf));
            }
            else if (tran.destinationStateMachine != null)
            {
                // create transition for cur state to all entry states in sub state machines
                List<Transition> entryTransitionList = new List<Transition>();
                CollectEntryTransitionToStates(entryTransitionList, null, tran.destinationStateMachine, true);
                if (entryTransitionList.Count > 0)
                {
                    List<ConfigCondition> tmp = new List<ConfigCondition>();
                    foreach (Transition entryTransition in entryTransitionList)
                    {
                        tmp.Clear();
                        tmp.AddRange(from condition in conditionList select condition.Copy());
                        tmp.AddRange(from condition in entryTransition.conditionList select condition.Copy());
                        ret.Add(new ConfigTransition(state?.name, entryTransition.toState, tmp.ToArray(), fadeDur, offset, false));
                    }
                }
            }
            else if (tran.isExit)
            {
                // transition to exit state
                if (parentStack.Count > 0)
                {
                    List<Transition> exitTransitionList = new List<Transition>();
                    List<AnimatorStateMachine> tmpParentStack = new List<AnimatorStateMachine>(parentStack);
                    CollectExitTransitionsToStates(exitTransitionList, null, tmpParentStack, sm);

                    List<ConfigCondition> tmp = new List<ConfigCondition>();
                    foreach (Transition exitTransition in exitTransitionList)
                    {
                        tmp.Clear();
                        tmp.AddRange(from condition in conditionList select condition.Copy());
                        tmp.AddRange(from condition in exitTransition.conditionList select condition.Copy());
                        ret.Add(new ConfigTransition(state?.name, exitTransition.toState, tmp.ToArray(), fadeDur, offset, false));
                    }
                }
                else
                {
                    // reach the root layer
                    List<Transition> entryTransitionList = new List<Transition>();
                    CollectEntryTransitionToStates(entryTransitionList, null, _baseSm, true);
                    if (entryTransitionList.Count > 0)
                    {
                        List<ConfigCondition> tmp = new List<ConfigCondition>();
                        foreach (Transition entryTransition in entryTransitionList)
                        {
                            tmp.Clear();
                            tmp.AddRange(from condition in conditionList select condition.Copy());
                            tmp.AddRange(from condition in entryTransition.conditionList select condition.Copy());
                            ret.Add(new ConfigTransition(state?.name, entryTransition.toState, tmp.ToArray(), fadeDur, offset, false));
                        }
                    }
                }
            }
        }

        private void CollectEntryTransitionToStates(List<Transition> ret, Transition parentTransition, AnimatorStateMachine sm, bool isRecursive)
        {
            // 因为AnimatorController不允许从entry到exit的连线，所以不用考虑entryTransition.isExit的情况
            foreach (AnimatorTransition entryTransition in sm.entryTransitions)
            {
                if (entryTransition.destinationState != null)
                {
                    // states that actually to a state
                    Transition transition = new Transition();
                    transition.toState = entryTransition.destinationState.name;
                    if (parentTransition != null)
                    {
                        transition.CombineConditions(parentTransition);
                    }
                    foreach (AnimatorCondition condition in entryTransition.conditions)
                    {
                        transition.conditionList.Add(ExportCondition(condition));
                    }
                    ret.Add(transition);
                }
                else if (entryTransition.destinationStateMachine != null && isRecursive)
                {
                    // deep into sub state machine
                    Transition transition = new Transition();
                    if (parentTransition != null)
                    {
                        transition.CombineConditions(parentTransition);
                    }
                    foreach (AnimatorCondition condition in entryTransition.conditions)
                    {
                        transition.conditionList.Add(ExportCondition(condition));
                    }
                    CollectEntryTransitionToStates(ret, transition, entryTransition.destinationStateMachine, isRecursive);
                }
            }
            if (sm.defaultState != null && ret.FindIndex((transition) => transition.toState == sm.defaultState.name) < 0)
            {
                ret.Add(new Transition()
                {
                    toState = sm.defaultState.name
                });
            }
        }

        private void CollectExitTransitionsToStates(List<Transition> ret, Transition exitTransitionInChild, List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm)
        {
            if (parentStack.Count == 0)
                return;
            AnimatorStateMachine parentSm = parentStack[parentStack.Count - 1];
            AnimatorTransition[] transitionsInParent = parentSm.GetStateMachineTransitions(sm);
            // 若当前子状态机在父状态机没有连线，退出时直接返回到父状态机的默认状态
            if (transitionsInParent.Length == 0)
            {
                if (parentSm.defaultState != null)
                {
                    Transition transition = new Transition() { toState = parentSm.defaultState.name };
                    if (exitTransitionInChild != null)
                    {
                        transition.CombineConditions(exitTransitionInChild);
                    }
                    ret.Add(transition);
                }
                return;
            }

            bool hasTransitionWithoutConditions = false;
            List<ConfigCondition> conditionList = new List<ConfigCondition>();
            foreach (AnimatorTransition transitionInParent in transitionsInParent)
            {
                conditionList.Clear();
                foreach (AnimatorCondition condition in transitionInParent.conditions)
                {
                    conditionList.Add(ExportCondition(condition));
                }
                if (conditionList.Count == 0)
                    hasTransitionWithoutConditions = true;

                if (transitionInParent.destinationState != null)
                {
                    Transition transition = new Transition() { toState = transitionInParent.destinationState.name };
                    if (exitTransitionInChild != null)
                    {
                        transition.CombineConditions(exitTransitionInChild);
                    }
                    transition.conditionList.AddRange(from condition in conditionList select condition.Copy());
                    ret.Add(transition);
                }
                else if (transitionInParent.destinationStateMachine != null)
                {
                    List<Transition> entryTransitionList = new List<Transition>();
                    CollectEntryTransitionToStates(entryTransitionList, null, transitionInParent.destinationStateMachine, true);
                    foreach (Transition entryTransition in entryTransitionList)
                    {
                        if (exitTransitionInChild != null)
                        {
                            entryTransition.CombineConditions(exitTransitionInChild);
                        }
                        entryTransition.conditionList.AddRange(from condition in conditionList select condition.Copy());
                        ret.Add(entryTransition);
                    }
                }
                else if (transitionInParent.isExit)
                {
                    // make a transition to parent sm
                    if (parentStack.Count > 1)
                    {

                        Transition transition = new Transition();
                        if (exitTransitionInChild != null)
                        {
                            transition.CombineConditions(exitTransitionInChild);
                        }
                        transition.conditionList.AddRange(from condition in conditionList select condition.Copy());
                        parentStack.RemoveAt(parentStack.Count - 1);
                        CollectExitTransitionsToStates(ret, transition, parentStack, parentSm);
                    }
                    else
                    {
                        // if reach the root layer
                        List<Transition> entryTransitionList = new List<Transition>();
                        CollectEntryTransitionToStates(entryTransitionList, null, _baseSm, true);
                        foreach (Transition entryTransition in entryTransitionList)
                        {
                            if (exitTransitionInChild != null)
                            {
                                entryTransition.CombineConditions(exitTransitionInChild);
                            }
                            entryTransition.conditionList.AddRange(from condition in conditionList select condition.Copy());
                            ret.Add(entryTransition);
                        }
                    }
                }
            }
            if (!hasTransitionWithoutConditions)
            {
                // make a transtion to default state
                Transition transition = new Transition() { toState = _defaultStateName };
                if (exitTransitionInChild != null)
                {
                    transition.CombineConditions(exitTransitionInChild);
                }
                ret.Add(transition);
            }
        }

        private ConfigCondition ExportCondition(AnimatorCondition cond)
        {
            var param = GetParameter(cond.parameter);
            if (param == null)
            {
                _hasError = true;
                Debug.LogErrorFormat("ExportCondition Fail Because Param Not Found {0}", cond.parameter);
                return null;
            }

            CompareMode mode = CompareMode.Equal;
            switch (cond.mode)
            {
                case AnimatorConditionMode.If:
                    mode = CompareMode.Equal;
                    break;
                case AnimatorConditionMode.IfNot:
                    mode = CompareMode.NotEqual;
                    break;
                case AnimatorConditionMode.Greater:
                    mode = CompareMode.Greater;
                    break;
                case AnimatorConditionMode.Less:
                    mode = CompareMode.Less;
                    break;
                case AnimatorConditionMode.Equals:
                    mode = CompareMode.Equal;
                    break;
                case AnimatorConditionMode.NotEqual:
                    mode = CompareMode.NotEqual;
                    break;
                default:
                    break;
            }

            ConfigCondition cfg = null;
            if (cond.parameter == "NormalizedTime")
            {
                cfg = new ConfigConditionByStateTime(cond.threshold, true, mode);
            }
            else if (cond.parameter == "Time")
            {
                cfg = new ConfigConditionByStateTime(cond.threshold, false, mode);
            }
            else
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Trigger:
                        {
                            cfg = new ConfigConditionByDataTrigger(cond.parameter);
                            break;
                        }
                    case AnimatorControllerParameterType.Bool:
                        {
                            cfg = new ConfigConditionByDataBool(cond.parameter, cond.mode == AnimatorConditionMode.If, CompareMode.Equal);
                            break;
                        }
                    case AnimatorControllerParameterType.Int:
                        {
                            cfg = new ConfigConditionByDataInt(cond.parameter, (int)cond.threshold, mode);
                            break;
                        }
                    case AnimatorControllerParameterType.Float:
                        {
                            cfg = new ConfigConditionByDataFloat(cond.parameter, cond.threshold, mode);
                            break;
                        }
                }
            }


            return cfg;
        }
    }
}