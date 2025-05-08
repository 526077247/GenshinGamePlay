using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 设置环境
    /// </summary>
    [LabelText("设置环境")]
    [NinoType(false)]
    public partial class ConfigSceneGroupSetEnvironmentAction : ConfigSceneGroupAction
    {
        [NinoMember(10)][LabelText("当前关卡该环境标识(用于移除)")][NotNull]
        public string Key;
        
        [NinoMember(11)][LabelText("进入环境还是移除")]
        public bool IsEnter;
        
        [NinoMember(12)][ShowIf(nameof(IsEnter))]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetEnvironmentConfigIds)+"()",AppendNextDrawer = true)]
#endif
        public int EnvId;
        
        [NinoMember(13)][ShowIf(nameof(IsEnter))]
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