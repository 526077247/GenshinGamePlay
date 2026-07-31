using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [LabelText("玩家进入触发区域")]
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [ProtoContract]
    public partial class ConfigEnterZoneEvtGetIsMyCondition : ConfigSceneGroupCondition<EnterZoneEvent>
    {

        public override bool IsMatch(EnterZoneEvent obj, SceneGroup sceneGroup)
        {
            var scene = SceneManager.Instance.CurrentScene as MapScene;
            if (scene != null)
                return obj.EntityId == scene.MyId;
            return false;
        }
#if UNITY_EDITOR
        protected override bool CheckModeType<T>(T t, CompareMode mode)
        {
            if (!base.CheckModeType(t, mode))
            {
                return false;
            }

            return true;
        }
#endif
    }
}