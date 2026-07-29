using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [TriggerType(typeof(ConfigAnyMonsterDieEventTrigger))]
    [LabelText("当怪物死亡后通过ActorId创建Monster")]
    [ProtoContract]
    public class ConfigSceneGroupCreateMonsterOnMonsterDieAction: ConfigSceneGroupAction
    {
        [ProtoMember(10, IsRequired = true)]
        public bool CreateDieMonster = true;
        [ProtoMember(11)]
#if UNITY_EDITOR
        [ShowIf("@!"+nameof(CreateDieMonster))]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetSceneGroupActorIds)+"()",AppendNextDrawer = true)]
#endif
        public int ActorId;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            if (CreateDieMonster)
            {
                if (evt is AnyMonsterDieEvent dieEvent)
                {
                    aimSceneGroup.CreateActor(dieEvent.ActorId);
                }
            }
            else
            {
                aimSceneGroup.CreateActor(ActorId);
            }
        }
    }
}