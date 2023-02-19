using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TaoTie
{
    // Trigger
    public abstract partial class ConfigGearTrigger
    {
        [PropertyOrder(int.MinValue)] 
        [NinoMember(1)]
        public int localId;
        [NinoMember(2)]
        [OnCollectionChanged(nameof(Refresh))] [OnStateUpdate(nameof(Refresh))] [SerializeReference]
        [PropertyOrder(int.MaxValue - 1)]
        [TypeFilter("@OdinDropdownHelper.GetFilteredActionTypeList(GetType())")]
        public ConfigGearAction[] actions;

#if UNITY_EDITOR
        [PropertyOrder(int.MinValue + 1)] [LabelText("策划备注")]
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

        public abstract void OnTrigger(Gear gear, IEventBase evt);
    }
    
    public abstract class ConfigGearTrigger<T> : ConfigGearTrigger where T : IEventBase
    {
        [JsonIgnore]
        private Type EventType => TypeInfo<T>.Type;

        public sealed override void OnTrigger(Gear gear, IEventBase evt)
        {
            if (evt.GetType() != EventType) return;
            Log.Info("OnTrigger: " + GetType().Name);
            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].ExecuteAction(evt, gear);
                }
            }
        }
    }
}