using System;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("玩家进入触发区域")]
    [TriggerType(typeof(ConfigExitZoneEventTrigger))]
    [NinoSerialize]
    public partial class ConfigExitZoneEvtGetIsMyCondition : ConfigSceneGroupCondition<ExitZoneEvent>
    {
        public override bool IsMatch(ExitZoneEvent obj, SceneGroup sceneGroup)
        {
            var scene = SceneManager.Instance.CurrentScene as BaseMapScene;
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