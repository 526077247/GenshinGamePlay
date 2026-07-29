using System;
using DaGenGraph;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    // Trigger
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigSceneGroupTrigger<AnyMonsterDieEvent>))]
    [ProtoInclude(101, typeof(ConfigSceneGroupTrigger<AvatarNearPlatformEvt>))]
    [ProtoInclude(102, typeof(ConfigSceneGroupTrigger<EnterZoneEvent>))]
    [ProtoInclude(103, typeof(ConfigSceneGroupTrigger<ExitZoneEvent>))]
    [ProtoInclude(104, typeof(ConfigSceneGroupTrigger<GadgetStateChangeEvt>))]
    [ProtoInclude(105, typeof(ConfigSceneGroupTrigger<GameTimeChange>))]
    [ProtoInclude(106, typeof(ConfigSceneGroupTrigger<PlatformReachPointEvt>))]
    [ProtoInclude(107, typeof(ConfigSceneGroupTrigger<StoryPlayOverEvt>))]
    [ProtoInclude(108, typeof(ConfigSceneGroupTrigger<SuiteLoadEvent>))]
    [ProtoInclude(109, typeof(ConfigSceneGroupTrigger<VariableChangeEvent>))]
    public abstract partial class ConfigSceneGroupTrigger
    {
        [PropertyOrder(int.MinValue)] 
        [ProtoMember(1)][DrawIgnore]
        public int LocalId;
        [ProtoMember(2)]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))] [OnStateUpdate(nameof(Refresh))] 
#endif
        [SerializeReference] [PropertyOrder(int.MaxValue - 1)][DrawIgnore]
#if UNITY_EDITOR
        [TypeFilter("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetFilteredActionTypeList)+"("+nameof(GetType)+"())")]
#endif
        public ConfigSceneGroupAction[] Actions;

#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
        public string Remarks;
        
        private void Refresh()
        {
            if (Actions == null) return;
            for (int i = 0; i < Actions.Length; i++)
            {
                if (Actions[i] != null)
                    Actions[i].HandleType = GetType();
            }

            Actions.Sort((a, b) => { return a.LocalId - b.LocalId; });
        }
#endif

        public abstract void OnTrigger(SceneGroup sceneGroup, IEventBase evt);
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigVariableChangeEventTrigger))]
    [ProtoInclude(101, typeof(ConfigSuiteLoadEventTrigger))]
    [ProtoInclude(102, typeof(ConfigStoryPlayOverEvtTrigger))]
    [ProtoInclude(103, typeof(ConfigPlatformReachPointEvtTrigger))]
    [ProtoInclude(104, typeof(ConfigGameTimeChangeTrigger))]
    [ProtoInclude(105, typeof(ConfigGadgetStateChangeEvtTrigger))]
    [ProtoInclude(106, typeof(ConfigExitZoneEventTrigger))]
    [ProtoInclude(107, typeof(ConfigEnterZoneEventTrigger))]
    [ProtoInclude(108, typeof(ConfigAvatarNearPlatformEvtTrigger))]
    [ProtoInclude(109, typeof(ConfigAnyMonsterDieEventTrigger))]
    public abstract class ConfigSceneGroupTrigger<T> : ConfigSceneGroupTrigger where T : IEventBase
    {
        [JsonIgnore]
        private Type EventType => TypeInfo<T>.Type;

        public sealed override void OnTrigger(SceneGroup sceneGroup, IEventBase evt)
        {
            if (evt.GetType() != EventType) return;
            if (!CheckCondition(sceneGroup, (T)evt)) return;
            Log.Info("OnTrigger: " + GetType().Name);
            if (Actions != null)
            {
                for (int i = 0; i < Actions.Length; i++)
                {
                    Actions[i].ExecuteAction(evt, sceneGroup, sceneGroup);
                }
            }
        }

        protected virtual bool CheckCondition(SceneGroup sceneGroup, T evt)
        {
            return true;
        }
    }
}