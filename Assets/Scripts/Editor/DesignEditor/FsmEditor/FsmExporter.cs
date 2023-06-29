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
        const string EditDirName = "Edit";
        const string PublishDirName = "Animations";
        
        const string ControllerConfigName = "FsmConfig.json";
        const string AIControllerConfigName = "PoseConfig.json";
        const string FsmConfig = EditDirName + "/" + "FsmConfig.controller";
        const string PoseConfig = EditDirName + "/" + "PoseConfig.controller";
        
        [MenuItem("Assets/工具/状态机导出", false)]
        static void ExportFsm()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Selection.activeObject is AnimatorController controller)
            {
                if (path.EndsWith(FsmConfig))
                {
                    ExportController(controller, ControllerConfigName);
                }
                else if (path.EndsWith(PoseConfig))
                {
                    ExportController(controller, AIControllerConfigName, false);
                }
            }
        }
        [MenuItem("Assets/工具/状态机导出", true)]
        static bool ExportFsmCheck()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Selection.activeObject is AnimatorController controller)
            {
                if (path.EndsWith(FsmConfig))
                {
                    return true;
                }
                else if (path.EndsWith(PoseConfig))
                {
                    return true;
                }
            }
            return false;
        }
        private static void ExportController(AnimatorController controller, string name, bool publish = true)
        {
            if (controller == null)
            {
                Debug.LogError("AnimatorController 不能为空");
                return;
            }

            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string editDir = Path.GetDirectoryName(controllerPath);
            if (Path.GetFileName(editDir) != EditDirName)
            {
                Debug.LogError("AnimatorController 不在编辑目录");
                return;
            }
            
            FsmExporter exporter = new FsmExporter(controller, editDir, publish);
            exporter.Generate(name);
        }


        private AnimatorController controller = null;
        private string fsmActionsPath = null;
        private bool publish = false;
        private Dictionary<string, ConfigFsmTimeline> fsmTimelineDict;
        private Dictionary<string, ConfigParam> paramDict;
        private bool hasError = false;
        private AnimatorStateMachine baseSm = null;
        private string defaultStateName = null;

        public FsmExporter(AnimatorController controller, string actionPath, bool publish)
        {
            this.publish = publish;
            this.controller = controller;
            fsmActionsPath = actionPath;
        }

        public void Generate(string _controllerConfigName)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string controllerSavePath = controllerPath.Replace(EditDirName, PublishDirName);
            string configSavePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(controllerPath)),
                _controllerConfigName);

            hasError = false;
            ExportController(controllerPath, controllerSavePath);
            ExportParam();

            LoadFsmTimeline();
            List<ConfigFsm> fsmList = new List<ConfigFsm>();
            for (int i = 0; i < controller.layers.Length; ++i)
            {
                if (controller.layers[i].syncedLayerIndex >= 0)
                    continue;
                var cfgFsm = ExportLayer(i);
                fsmList.Add(cfgFsm);
            }

            ConfigFsmController newController = new ConfigFsmController();

            newController.FsmConfigs = fsmList.ToArray();
            newController.ParamDict = paramDict;

            if (!hasError)
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
            AnimatorControllerExporter.Export(srcPath, savePath, publish);
        }

        #endregion

        #region Fsm Timeline

        private void LoadFsmTimeline()
        {
            fsmTimelineDict = new Dictionary<string, ConfigFsmTimeline>();
            LoadFsmTimelineInPath(fsmActionsPath);
        }

        private void LoadFsmTimelineInPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
                return;

            FileInfo[] fileInfos = dirInfo.GetFiles("*.playable", SearchOption.TopDirectoryOnly);
            foreach (FileInfo fileInfo in fileInfos)
            {
                ConfigFsmTimeline timeline = TimelineSerializer.GetFsmFromTimeline(Path.Combine(path, fileInfo.Name));
                fsmTimelineDict.Add(fileInfo.Name.Split('.')[0], timeline);
            }
        }

        #endregion

        #region Fsm Param

        private Dictionary<string, ConfigParam> ExportParam()
        {
            paramDict = new Dictionary<string, ConfigParam>();
            foreach (var param in controller.parameters)
            {
                var paramCfg = GenerateParameters(param);
                if (paramCfg != null)
                {
                    paramDict[paramCfg.Key] = paramCfg;
                }
            }

            return paramDict;
        }

        private ConfigParam GenerateParameters(AnimatorControllerParameter param)
        {
            bool animUse = AnimatorControllerExporter.animatorControllerParameterUsed.ContainsKey(param.name);

            switch (param.type)
            {
                case AnimatorControllerParameterType.Bool:
                {
                    return new ConfigParamBool
                        {Key = param.name, defaultValue = param.defaultBool, NeedSyncAnimator = animUse};
                }
                case AnimatorControllerParameterType.Trigger:
                {
                    return new ConfigParamTrigger
                        {Key = param.name, defaultValue = param.defaultBool, NeedSyncAnimator = animUse};
                }
                case AnimatorControllerParameterType.Int:
                {
                    return new ConfigParamInt
                        {Key = param.name, defaultValue = param.defaultInt, NeedSyncAnimator = animUse};
                }
                case AnimatorControllerParameterType.Float:
                {
                    return new ConfigParamFloat
                        {Key = param.name, defaultValue = param.defaultFloat, NeedSyncAnimator = animUse};
                }
                default:
                    break;
            }

            return null;
        }

        private AnimatorControllerParameter GetParameter(string name)
        {
            return Array.Find(controller.parameters, a => a.name == name);
        }

        #endregion

        private ConfigFsm ExportLayer(int layerIndex)
        {
            AnimatorControllerLayer layer = controller.layers[layerIndex];
            List<ConfigFsmState> stateList = new List<ConfigFsmState>();
            List<ConfigTransition> anyStateTransitionList = new List<ConfigTransition>();

            List<AnimatorStateMachine> parentStack = new List<AnimatorStateMachine>();
            baseSm = layer.stateMachine;
            defaultStateName = layer.stateMachine.defaultState.name;
            ExportStateMachine(parentStack, layer.stateMachine, ref stateList, ref anyStateTransitionList);

            ConfigFsm cfgFsm = new ConfigFsm{Name = layer.name, LayerIndex = layerIndex};
            cfgFsm.Entry = defaultStateName;
            cfgFsm.StateDict = new Dictionary<string, ConfigFsmState>();
            foreach (var state in stateList)
            {
                cfgFsm.StateDict.Add(state.Name, state);
            }
            cfgFsm.AnyStateTransitions = anyStateTransitionList.ToArray();

            return cfgFsm;
        }

        private void ExportStateMachine(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm,
            ref List<ConfigFsmState> stateList, ref List<ConfigTransition> anyStateTransitionList)
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

        private void ExportAnyStateTransitions(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm,
            ref List<ConfigTransition> anyStateTransitionList)
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

        private ConfigFsmState ExportState(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm,
            AnimatorState state)
        {
            List<ConfigTransition> transList = new List<ConfigTransition>();
            foreach (var tran in state.transitions)
            {
                ExportStateTransition(parentStack, sm, state, tran, false, ref transList);
            }

            ConfigFsmTimeline timeline;
            fsmTimelineDict.TryGetValue(state.name, out timeline);

            float stateDuration = 1f;
            var stateLoop = false;
            if (state.motion != null)
            {
                stateLoop = state.motion.isLooping;
                stateDuration = state.motion.averageDuration;
            }

            ConfigFsmState ret = new ConfigFsmState
            {
                Name = state.name, 
                StateDuration = stateDuration,
                StateLoop = stateLoop,
                Transitions = transList.ToArray(),
                Timeline = timeline,
            };
            if (state.behaviours.Length > 0)
            {
                for (int i = 0; i < state.behaviours.Length; i++)
                {
                    if (state.behaviours[i] is AnimatorParam param)
                    {
                        ret.Data = param.Data;
                    }
                }
            }

            return ret;
        }

        private void ExportStateTransition(List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm,
            AnimatorState state, AnimatorStateTransition tran, bool isAnyStateTransition,
            ref List<ConfigTransition> ret)
        {
            List<ConfigCondition> conditionList = new List<ConfigCondition>();
            foreach (var item in tran.conditions)
            {
                conditionList.Add(ExportCondition(item));
            }

            if (tran.hasExitTime)
            {
                conditionList.Add(new ConfigConditionByStateTime
                    {Time = tran.exitTime, IsNormalized = true, Mode = CompareMode.GEqual});
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
                Debug.LogWarning(
                    $"[FsmExporter:ExportTransition] transition {tran.name} can not use fade duration in normalized time");
            }

            float offset = tran.offset;

            if (tran.destinationState != null)
            {
                if (tran.destinationState.motion != null)
                {
                    offset *= tran.destinationState.motion.averageDuration;
                }

                bool canTransitionToSelf = isAnyStateTransition
                    ? tran.canTransitionToSelf
                    : state?.name == tran.destinationState.name;
                var conditions = RemoveDuplicates(conditionList);
                if (!IsMutualExclusion(conditions))
                {
                    ret.Add(new ConfigTransition
                    {
                        FromState = state?.name,
                        ToState = tran.destinationState.name,
                        Conditions = conditions,
                        FadeDuration = fadeDur,
                        ToStateTime = offset,
                        CanTransitionToSelf = canTransitionToSelf,
                        InteractionSource = (TransitionInterruptionSource)(int)tran.interruptionSource,
                        OrderedInteraction = tran.orderedInterruption
                    });
                }
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
                        var conditions = RemoveDuplicates(tmp);
                        if (!IsMutualExclusion(conditions))
                        {
                            ret.Add(new ConfigTransition
                            {
                                FromState = state?.name,
                                ToState = entryTransition.toState,
                                Conditions = conditions,
                                FadeDuration = fadeDur,
                                ToStateTime = offset,
                                CanTransitionToSelf = false,
                                InteractionSource = (TransitionInterruptionSource)(int)tran.interruptionSource,
                                OrderedInteraction = tran.orderedInterruption
                            });
                        }
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
                        var conditions = RemoveDuplicates(tmp);
                        if (!IsMutualExclusion(conditions))
                        {
                            ret.Add(new ConfigTransition
                            {
                                FromState = state?.name,
                                ToState = exitTransition.toState,
                                Conditions = conditions,
                                FadeDuration = fadeDur,
                                ToStateTime = offset,
                                CanTransitionToSelf = false,
                                InteractionSource = (TransitionInterruptionSource)(int)tran.interruptionSource,
                                OrderedInteraction = tran.orderedInterruption
                            });
                        }
                    }
                }
                else
                {
                    // reach the root layer
                    List<Transition> entryTransitionList = new List<Transition>();
                    CollectEntryTransitionToStates(entryTransitionList, null, baseSm, true);
                    if (entryTransitionList.Count > 0)
                    {
                        List<ConfigCondition> tmp = new List<ConfigCondition>();
                        foreach (Transition entryTransition in entryTransitionList)
                        {
                            tmp.Clear();
                            tmp.AddRange(from condition in conditionList select condition.Copy());
                            tmp.AddRange(from condition in entryTransition.conditionList select condition.Copy());

                            var conditions = RemoveDuplicates(tmp);
                            if (!IsMutualExclusion(conditions))
                            {
                                ret.Add(new ConfigTransition
                                {
                                    FromState = state?.name,
                                    ToState = entryTransition.toState,
                                    Conditions = conditions,
                                    FadeDuration = fadeDur,
                                    ToStateTime = offset,
                                    CanTransitionToSelf = false,
                                    InteractionSource = (TransitionInterruptionSource)(int)tran.interruptionSource,
                                    OrderedInteraction = tran.orderedInterruption
                                });
                            }
                        }
                    }
                }
            }
        }

        private void CollectEntryTransitionToStates(List<Transition> ret, Transition parentTransition,
            AnimatorStateMachine sm, bool isRecursive)
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

                    CollectEntryTransitionToStates(ret, transition, entryTransition.destinationStateMachine,
                        isRecursive);
                }
            }

            if (sm.defaultState != null &&
                ret.FindIndex((transition) => transition.toState == sm.defaultState.name) < 0)
            {
                ret.Add(new Transition()
                {
                    toState = sm.defaultState.name
                });
            }
        }

        private void CollectExitTransitionsToStates(List<Transition> ret, Transition exitTransitionInChild,
            List<AnimatorStateMachine> parentStack, AnimatorStateMachine sm)
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
                    Transition transition = new Transition() {toState = parentSm.defaultState.name};
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
                    Transition transition = new Transition() {toState = transitionInParent.destinationState.name};
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
                    CollectEntryTransitionToStates(entryTransitionList, null,
                        transitionInParent.destinationStateMachine, true);
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
                        CollectEntryTransitionToStates(entryTransitionList, null, baseSm, true);
                        foreach (Transition entryTransition in entryTransitionList)
                        {
                            if (exitTransitionInChild != null)
                            {
                                entryTransition.CombineConditions(exitTransitionInChild);
                            }

                            entryTransition.conditionList.AddRange(from condition in conditionList
                                select condition.Copy());
                            ret.Add(entryTransition);
                        }
                    }
                }
            }

            if (!hasTransitionWithoutConditions)
            {
                // make a transition to default state
                Transition transition = new Transition() {toState = defaultStateName};
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
                hasError = true;
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
                cfg = new ConfigConditionByStateTime{Time = cond.threshold,IsNormalized = true,Mode = mode};
            }
            else if (cond.parameter == "Time")
            {
                cfg = new ConfigConditionByStateTime{Time = cond.threshold,IsNormalized = false,Mode = mode};
            }
            else
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Trigger:
                    {
                        cfg = new ConfigConditionByDataTrigger{Key = cond.parameter};
                        break;
                    }
                    case AnimatorControllerParameterType.Bool:
                    {
                        cfg = new ConfigConditionByDataBool
                        {
                            Key = cond.parameter, 
                            Value = cond.mode == AnimatorConditionMode.If,
                            Mode = CompareMode.Equal
                        };
                        break;
                    }
                    case AnimatorControllerParameterType.Int:
                    {
                        cfg = new ConfigConditionByDataInt
                        {
                            Key = cond.parameter,
                            Value = (int) cond.threshold,
                            Mode = mode
                        };
                        break;
                    }
                    case AnimatorControllerParameterType.Float:
                    {
                        cfg = new ConfigConditionByDataFloat
                        {
                            Key = cond.parameter,
                            Value = cond.threshold,
                            Mode = mode
                        };
                        break;
                    }
                }
            }


            return cfg;
        }
        /// <summary>
        /// 条件去重
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private ConfigCondition[] RemoveDuplicates(List<ConfigCondition> source)
        {
            List<ConfigCondition> set = new List<ConfigCondition>();
            for (int i = 0; i < source.Count; i++)
            {
                bool has = false;
                for (int j = 0; j < set.Count; j++)
                {
                    if (set[j].Equals(source[i]))
                    {
                        has = true;
                        break;
                    }
                }

                if (!has)
                {
                    set.Add(source[i]);
                }
            }

            return set.ToArray();
        }

        /// <summary>
        /// 判断是否有互斥条件
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private bool IsMutualExclusion(ConfigCondition[] source)
        {
            UnOrderMultiMap<string, ConfigCondition> map = new UnOrderMultiMap<string, ConfigCondition>();
            for (int i = 0; i < source.Length; i++)
            {
                var cond = source[i];
                if (cond is ConfigConditionByData a)
                {
                    if (map.TryGetValue(a.Key, out var oldList))
                    {
                        for (int j = 0; j < oldList.Count; j++)
                        {
                            var old = oldList[j];

                            if (old is ConfigConditionByDataBool dataBoola &&
                                cond is ConfigConditionByDataBool dataBoolb)
                            {
                                if (dataBoola.Value != dataBoolb.Value)
                                {
                                    return true;
                                }
                            }
                            else if (old is ConfigConditionByDataFloat dataFloata &&
                                     cond is ConfigConditionByDataFloat dataFloatb)
                            {
                                if (dataFloata.Value == dataFloatb.Value)
                                {
                                    if (dataFloata.Mode == CompareMode.Equal &&
                                        (dataFloatb.Mode == CompareMode.NotEqual ||
                                         dataFloatb.Mode == CompareMode.Less ||
                                         dataFloatb.Mode == CompareMode.NotEqual))
                                    {
                                        return true;
                                    }

                                    if (dataFloata.Mode == CompareMode.NotEqual && dataFloatb.Mode == CompareMode.Equal)
                                    {
                                        return true;
                                    }

                                    if (dataFloata.Mode == CompareMode.Greater &&
                                        (dataFloatb.Mode == CompareMode.Less || dataFloatb.Mode == CompareMode.LEqual ||
                                         dataFloatb.Mode == CompareMode.Equal))
                                    {
                                        return true;
                                    }

                                    if (dataFloata.Mode == CompareMode.GEqual && dataFloatb.Mode == CompareMode.Less)
                                    {
                                        return true;
                                    }

                                    if (dataFloata.Mode == CompareMode.Less &&
                                        (dataFloatb.Mode == CompareMode.Greater ||
                                         dataFloatb.Mode == CompareMode.GEqual || dataFloatb.Mode == CompareMode.Equal))
                                    {
                                        return true;
                                    }

                                    if (dataFloata.Mode == CompareMode.LEqual && dataFloatb.Mode == CompareMode.Greater)
                                    {
                                        return true;
                                    }
                                }
                            }
                            else if (old is ConfigConditionByDataInt dataInta &&
                                     cond is ConfigConditionByDataInt dataIntb)
                            {
                                if (dataInta.Value == dataIntb.Value)
                                {
                                    if (dataInta.Mode == CompareMode.Equal && (dataIntb.Mode == CompareMode.NotEqual ||
                                                                               dataIntb.Mode == CompareMode.Less ||
                                                                               dataIntb.Mode == CompareMode.NotEqual))
                                    {
                                        return true;
                                    }

                                    if (dataInta.Mode == CompareMode.NotEqual && dataIntb.Mode == CompareMode.Equal)
                                    {
                                        return true;
                                    }

                                    if (dataInta.Mode == CompareMode.Greater && (dataIntb.Mode == CompareMode.Less ||
                                            dataIntb.Mode == CompareMode.LEqual || dataIntb.Mode == CompareMode.Equal))
                                    {
                                        return true;
                                    }

                                    if (dataInta.Mode == CompareMode.GEqual && dataIntb.Mode == CompareMode.Less)
                                    {
                                        return true;
                                    }

                                    if (dataInta.Mode == CompareMode.Less && (dataIntb.Mode == CompareMode.Greater ||
                                                                              dataIntb.Mode == CompareMode.GEqual ||
                                                                              dataIntb.Mode == CompareMode.Equal))
                                    {
                                        return true;
                                    }

                                    if (dataInta.Mode == CompareMode.LEqual && dataIntb.Mode == CompareMode.Greater)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    map.Add(a.Key,a);
                }
            }

            return false;
        }
    }
}