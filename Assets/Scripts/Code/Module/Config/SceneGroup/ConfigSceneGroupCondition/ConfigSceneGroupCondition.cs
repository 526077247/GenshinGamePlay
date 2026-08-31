using System;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaoTie
{
    // Condition
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigGetSuiteMonsterCountCondition))]
    [ProtoInclude(101, typeof(ConfigVariableCondition))]
    [ProtoInclude(102, typeof(ConfigSceneGroupCondition<EnterZoneEvent>))]
    [ProtoInclude(103, typeof(ConfigSceneGroupCondition<ExitZoneEvent>))]
    [ProtoInclude(104, typeof(ConfigSceneGroupCondition<AnyMonsterDieEvent>))]
    [ProtoInclude(105, typeof(ConfigSceneGroupCondition<AvatarNearPlatformEvt>))]
    [ProtoInclude(106, typeof(ConfigSceneGroupCondition<GadgetStateChangeEvt>))]
    [ProtoInclude(107, typeof(ConfigSceneGroupCondition<GameTimeChange>))]
    [ProtoInclude(108, typeof(ConfigSceneGroupCondition<PlatformReachPointEvt>))]
    [ProtoInclude(109, typeof(ConfigSceneGroupCondition<StoryPlayOverEvt>))]
    [ProtoInclude(110, typeof(ConfigSceneGroupCondition<SuiteLoadEvent>))]
    [ProtoInclude(111, typeof(ConfigSceneGroupCondition<SuiteUnloadEvent>))]
    public abstract partial class ConfigSceneGroupCondition
    {
        public abstract bool IsMatch(IEventBase obj, SceneGroup sceneGroup);

        public bool IsMatch(Enum configValue, Enum evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue.Equals(configValue);
                case CompareMode.NotEqual:
                    return !evtValue.Equals(configValue);
                default:
                    Log.Error("Enum类型不支持" + mode);
                    return false;
            }
        }
        
        public bool IsMatch(int configValue, int evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                case CompareMode.Greater:
                    return evtValue > configValue;
                case CompareMode.Less:
                    return evtValue < configValue;
                case CompareMode.LEqual:
                    return evtValue <= configValue;
                case CompareMode.GEqual:
                    return evtValue >= configValue;
                default:
                    return false;
            }
        }

        public bool IsMatch(float configValue, float evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                case CompareMode.Greater:
                    return evtValue > configValue;
                case CompareMode.Less:
                    return evtValue < configValue;
                case CompareMode.LEqual:
                    return evtValue <= configValue;
                case CompareMode.GEqual:
                    return evtValue >= configValue;
                default:
                    return false;
            }
        }

        public bool IsMatch(string configValue, string evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                default:
                    Log.Error("string类型不支持" + mode);
                    return false;
            }
        }

        protected bool IsMatch(bool configValue, bool evtValue, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.Equal:
                    return evtValue == configValue;
                case CompareMode.NotEqual:
                    return evtValue != configValue;
                default:
                    Log.Error("bool类型不支持" + mode);
                    return false;
            }
        }
        

        public int GetLogicValue(int from, int value, LogicMode mode)
        {
            switch (mode)
            {
                case LogicMode.Add:
                    return from + value;
                case LogicMode.Red:
                    return from - value;
                case LogicMode.Mul:
                    return from * value;
                case LogicMode.Div:
                    if (value == 0) return from;
                    return from / value;
                case LogicMode.Rem:
                    if (value == 0) return from;
                    return from % value;
                case LogicMode.Pow:
                    return (int)Mathf.Pow(from,value);
                case LogicMode.Default:
                    return from;
                default:
                    Log.Error("类型不支持" + mode);
                    return from;
            }
        }
        public float GetLogicValue(float from, float value, LogicMode mode)
        {
            switch (mode)
            {
                case LogicMode.Add:
                    return from + value;
                case LogicMode.Red:
                    return from - value;
                case LogicMode.Mul:
                    return from * value;
                case LogicMode.Div:
                    if (value == 0) return from;
                    return from / value;
                case LogicMode.Rem:
                    if (value == 0) return from;
                    return from % value;
                case LogicMode.Pow:
                    return (int)Mathf.Pow(from,value);
                default:
                    Log.Error("类型不支持" + mode);
                    return from;
            }
        }

#if UNITY_EDITOR
        [LabelText("策划备注")] [PropertyOrder(int.MinValue + 1)]
        public string Remarks;

        public static void ShowNotification(string tips)
        {
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }

        
        protected virtual bool CheckModeType<T>(T t, CompareMode mode)
        {
            Type type = TypeInfo<T>.Type;
            if (type == TypeInfo<int>.Type || type == TypeInfo<short>.Type || type == TypeInfo<long>.Type ||
                type == TypeInfo<byte>.Type || type == TypeInfo<uint>.Type || type == TypeInfo<ushort>.Type ||
                type == TypeInfo<ulong>.Type || type == TypeInfo<sbyte>.Type || type == TypeInfo<float>.Type ||
                type == TypeInfo<double>.Type || type == TypeInfo<decimal>.Type)
            {
                return true;
            }

            if (mode != CompareMode.Equal && mode != CompareMode.NotEqual)
            {
                var str = type.Name + "类型不支持CompareMode." + mode;
                Log.Error(str);
                ShowNotification(str);
                return false;
            }

            return true;
        }

#endif
    }
    
    [ProtoContract]
    [ProtoInclude(102, typeof(ConfigSuiteLoadEventIsAddOnCondition))]
    [ProtoInclude(103, typeof(ConfigSuiteLoadEventSuiteIdCondition))]
    [ProtoInclude(104, typeof(ConfigStoryPlayOverEvtStoryIdCondition))]
    [ProtoInclude(105, typeof(ConfigPlatformReachPointEvtActorIdCondition))]
    [ProtoInclude(106, typeof(ConfigPlatformReachPointEvtPointIndexCondition))]
    [ProtoInclude(107, typeof(ConfigPlatformReachPointEvtRouteIdCondition))]
    [ProtoInclude(108, typeof(ConfigGameTimeChangeGameTimeNowCondition))]
    [ProtoInclude(109, typeof(ConfigGadgetStateChangeEvtGadgetIdCondition))]
    [ProtoInclude(110, typeof(ConfigGadgetStateChangeEvtOldStateCondition))]
    [ProtoInclude(111, typeof(ConfigGadgetStateChangeEvtStateCondition))]
    [ProtoInclude(112, typeof(ConfigAvatarNearPlatformEvtActorIdCondition))]
    [ProtoInclude(113, typeof(ConfigAvatarNearPlatformEvtIsMovingCondition))]
    [ProtoInclude(114, typeof(ConfigAvatarNearPlatformEvtPointIndexCondition))]
    [ProtoInclude(115, typeof(ConfigAvatarNearPlatformEvtRouteIdCondition))]
    [ProtoInclude(116, typeof(ConfigAnyMonsterDieEventActorIdCondition))]
    [ProtoInclude(117, typeof(ConfigExitZoneEvtGetIsMyCondition))]
    [ProtoInclude(118, typeof(ConfigExitZoneEvtGetRegionEntityCountCondition))]
    [ProtoInclude(119, typeof(ConfigExitZoneEventZoneLocalIdCondition))]
    [ProtoInclude(120, typeof(ConfigEnterZoneEvtGetIsMyCondition))]
    [ProtoInclude(121, typeof(ConfigEnterZoneEvtGetRegionEntityCountCondition))]
    [ProtoInclude(122, typeof(ConfigEnterZoneEventZoneLocalIdCondition))]
    [ProtoInclude(123, typeof(ConfigSuiteUnloadEventIsAddOnCondition))]
    [ProtoInclude(124, typeof(ConfigSuiteUnloadEventSuiteIdCondition))]
    public abstract class ConfigSceneGroupCondition<T>:ConfigSceneGroupCondition where T:IEventBase
    {
        [JsonIgnore]
        private Type EventType => TypeInfo<T>.Type;
        public sealed override bool IsMatch(IEventBase obj, SceneGroup sceneGroup)
        {
            if (EventType != obj.GetType()) return false;
            return IsMatch((T)obj, sceneGroup);
        }
        
        public abstract bool IsMatch(T obj, SceneGroup sceneGroup);
    }
}