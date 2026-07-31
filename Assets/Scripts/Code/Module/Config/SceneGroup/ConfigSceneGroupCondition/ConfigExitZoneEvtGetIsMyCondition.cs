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
    [TriggerType(typeof(ConfigExitZoneEventTrigger))]
    [ProtoContract]
    public partial class ConfigExitZoneEvtGetIsMyCondition : ConfigSceneGroupCondition<ExitZoneEvent>
    {
        public override bool IsMatch(ExitZoneEvent obj, SceneGroup sceneGroup)
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