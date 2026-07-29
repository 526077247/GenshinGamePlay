using System;
using TaoTie.LitJson.Extensions;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigSceneGroupAddExtraSuiteAction))]
    [ProtoInclude(101, typeof(ConfigSceneGroupAddVariableAction))]
    [ProtoInclude(102, typeof(ConfigSceneGroupCreateEntityByActorIdAction))]
    [ProtoInclude(103, typeof(ConfigSceneGroupCreateMonsterOnMonsterDieAction))]
    [ProtoInclude(104, typeof(ConfigSceneGroupDelayAction))]
    [ProtoInclude(105, typeof(ConfigSceneGroupGoToSuiteAction))]
    [ProtoInclude(106, typeof(ConfigSceneGroupOverAction))]
    [ProtoInclude(107, typeof(ConfigSceneGroupPlayStoryAction))]
    [ProtoInclude(108, typeof(ConfigSceneGroupPrintContextLogAction))]
    [ProtoInclude(109, typeof(ConfigSceneGroupReleaseEntityByActorIdAction))]
    [ProtoInclude(110, typeof(ConfigSceneGroupRemoveExtraSuiteAction))]
    [ProtoInclude(111, typeof(ConfigSceneGroupRestartPlatformMove))]
    [ProtoInclude(112, typeof(ConfigSceneGroupResumePlatformMove))]
    [ProtoInclude(113, typeof(ConfigSceneGroupSetEnvironmentAction))]
    [ProtoInclude(114, typeof(ConfigSceneGroupSetGadgetStateAction))]
    [ProtoInclude(115, typeof(ConfigSceneGroupTransferAction))]
    [ProtoInclude(116, typeof(ConfigSceneGroupConditionAction))]
    public abstract partial class ConfigSceneGroupAction
    {
        [ProtoMember(1)]
        [LabelText("禁用")] public bool Disable;

#if UNITY_EDITOR
        [HideInInspector][JsonIgnore][ProtoIgnore]
        public Type HandleType;
#endif
        [ProtoMember(2)]
        [LabelText("排序序号")] public int LocalId;
        [JsonIgnore]
        public virtual bool CanSetOtherSceneGroup { get; } = false;
        [ShowIf(nameof(CanSetOtherSceneGroup))] [LabelText("是否是设置其他SceneGroup的内容")] 
        [ProtoMember(3)]
        public bool IsOtherSceneGroup;

        [ShowIf(nameof(IsOtherSceneGroup))]
        [ProtoMember(4)]
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