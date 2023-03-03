using System;
using LitJson.Extensions;
using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TaoTie
{
    public abstract partial class ConfigSceneGroupAction
    {
        [NinoMember(1)]
        [LabelText("禁用")] public bool disable;

#if UNITY_EDITOR
        [HideInInspector][JsonIgnore]
        public Type handleType;
#endif
        [NinoMember(2)]
        [LabelText("排序序号")] public int localId;
        [JsonIgnore]
        public virtual bool canSetOtherSceneGroup { get; } = false;
        [ShowIf(nameof(canSetOtherSceneGroup))] [LabelText("是否是设置其他SceneGroup的内容")] 
        [NinoMember(3)]
        public bool isOtherSceneGroup;

        [ShowIf(nameof(isOtherSceneGroup))]
        [NinoMember(4)]
        public ulong othersceneGroupId;
        public void ExecuteAction(IEventBase evt, SceneGroup sceneGroup)
        {
            if (disable)
            {
                return;
            }

            var aimSceneGroup = sceneGroup;
            if (isOtherSceneGroup)
            {
                if (sceneGroup.manager.TryGetSceneGroup(othersceneGroupId, out var other))
                {
                    aimSceneGroup = other;
                }
                else
                {
                    Log.Error("未找到其他SceneGroup,请检查配置! id=" + othersceneGroupId);
                    return;
                }
            }

            Execute(evt, aimSceneGroup, sceneGroup);
        }

        protected abstract void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup);
    }
}