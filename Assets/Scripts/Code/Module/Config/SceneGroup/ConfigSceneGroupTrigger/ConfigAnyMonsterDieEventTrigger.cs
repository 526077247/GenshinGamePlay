using System;
using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当关卡内怪物死亡")]
    [ProtoContract]
    public partial class ConfigAnyMonsterDieEventTrigger : ConfigSceneGroupTrigger<AnyMonsterDieEvent>
    {
        
    }
}