using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    // Trigger
    public abstract partial class ConfigSceneGroupTrigger
    {
        [PropertyOrder(int.MinValue)] 
        [NinoMember(1)]
        public int localId;
        [NinoMember(2)]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(Refresh))] [OnStateUpdate(nameof(Refresh))] 
#endif
        [SerializeReference] [PropertyOrder(int.MaxValue - 1)]
        [TypeFilter("@OdinDropdownHelper.GetFilteredActionTypeList(GetType())")]
        public ConfigSceneGroupAction[] actions;

#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [SerializeField] [LabelText("策划备注")]
        private string remarks;
        
        private void Refresh()
        {
            if (actions == null) return;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                    actions[i].handleType = GetType();
            }

            actions.Sort((a, b) => { return a.localId - b.localId; });
        }
#endif

        public abstract void OnTrigger(SceneGroup sceneGroup, IEventBase evt);
    }
    
    public abstract class ConfigSceneGroupTrigger<T> : ConfigSceneGroupTrigger where T : IEventBase
    {
        [JsonIgnore]
        private Type EventType => TypeInfo<T>.Type;

        public sealed override void OnTrigger(SceneGroup sceneGroup, IEventBase evt)
        {
            if (evt.GetType() != EventType) return;
            if (!CheckCondition(sceneGroup, (T)evt)) return;
            Log.Info("OnTrigger: " + GetType().Name);
            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].ExecuteAction(evt, sceneGroup);
                }
            }
        }

        protected virtual bool CheckCondition(SceneGroup sceneGroup, T evt)
        {
            return true;
        }
    }
}