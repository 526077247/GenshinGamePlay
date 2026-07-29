using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 设置环境
    /// </summary>
    [LabelText("设置环境")]
    [ProtoContract]
    public partial class ConfigSceneGroupSetEnvironmentAction : ConfigSceneGroupAction
    {
        [ProtoMember(10)][LabelText("当前关卡该环境标识(用于移除)")][NotNull]
        public string Key;
        
        [ProtoMember(11)][LabelText("进入环境还是移除")]
        public bool IsEnter;
        
        [ProtoMember(12)][ShowIf(nameof(IsEnter))]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetEnvironmentConfigIds)+"()",AppendNextDrawer = true)]
#endif
        public int EnvId;
        
        [ProtoMember(13)][ShowIf(nameof(IsEnter))]
        public EnvironmentPriorityType Type;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (IsEnter)
            {
                aimSceneGroup.PushEnvironment(EnvId, Type, Key);
            }
            else
            {
                aimSceneGroup.RemoveEnvironment(Key);
            }
        }
    }
}