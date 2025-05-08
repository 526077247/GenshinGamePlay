using System;
using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [LabelText("通过ActorId创建实体")]
    [NinoType(false)]
    public partial class ConfigSceneGroupCreateEntityByActorIdAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int ActorId;
        [NinoMember(11)][MinValue(1)]
        public int Count = 1;
        
        [NinoMember(12)]
#if UNITY_EDITOR
        [MinValue(0.1f)]
        [LabelText("出生区域随机范围")]
        [ShowIf("@"+nameof(ShowRange)+"()")]
#endif
        public float Range = 1;
        
#if UNITY_EDITOR
        private bool ShowRange()
        {
            return Count > 1;
        }
#endif
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            for (int i = 0; i < Count; i++)
            {
                aimSceneGroup.CreateActor(ActorId, i != 0 ? Range : 0);
            }
        }
    }
}