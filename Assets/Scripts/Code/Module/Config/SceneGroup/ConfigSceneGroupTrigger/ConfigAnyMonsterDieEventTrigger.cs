using System;
using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当关卡内怪物死亡")]
    [ProtoContract]
    public partial class ConfigAnyMonsterDieEventTrigger : ConfigSceneGroupTrigger<AnyMonsterDieEvent>
    {
        
    }
}