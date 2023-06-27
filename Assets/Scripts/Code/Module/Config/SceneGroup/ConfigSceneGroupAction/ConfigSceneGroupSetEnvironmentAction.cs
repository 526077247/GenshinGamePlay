using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    /// <summary>
    /// 设置环境
    /// </summary>
    [LabelText("设置环境")]
    [NinoSerialize]
    public partial class ConfigSceneGroupSetEnvironmentAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        public string Key;
        
        [NinoMember(11)]
        public bool IsEnter;
        
        [NinoMember(12)][ShowIf(nameof(IsEnter))]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetEnvironmentConfigIds)+"()")]
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