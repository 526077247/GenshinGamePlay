using System;
using TaoTie.LitJson.Extensions;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigSceneGroupAction
    {
        [NinoMember(1)]
        [LabelText("禁用")] public bool Disable;

#if UNITY_EDITOR
        [HideInInspector][JsonIgnore][NinoIgnore]
        public Type HandleType;
#endif
        [NinoMember(2)]
        [LabelText("排序序号")] public int LocalId;
        [JsonIgnore][NinoIgnore]
        public virtual bool CanSetOtherSceneGroup { get; } = false;
        [ShowIf(nameof(CanSetOtherSceneGroup))] [LabelText("是否是设置其他SceneGroup的内容")] 
        [NinoMember(3)]
        public bool IsOtherSceneGroup;

        [ShowIf(nameof(IsOtherSceneGroup))]
        [NinoMember(4)]
        public ulong OtherSceneGroupId;
        public void ExecuteAction(IEventBase evt, SceneGroup sceneGroup, SceneGroup fromSceneGroup)
        {
            if (Disable)
            {
                return;
            }

            var aimSceneGroup = sceneGroup;
            if (IsOtherSceneGroup)
            {
                if (sceneGroup.Manager.TryGetSceneGroup(OtherSceneGroupId, out var other))
                {
                    aimSceneGroup = other;
                }
                else
                {
                    Log.Error("未找到其他SceneGroup,请检查配置! id=" + OtherSceneGroupId);
                    return;
                }
            }

            Execute(evt, aimSceneGroup, fromSceneGroup);
        }

        protected abstract void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup);
    }
}