using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("玩家进入触发区域")]
    [TriggerType(typeof(ConfigEnterZoneEventTrigger))]
    [NinoType(false)]
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