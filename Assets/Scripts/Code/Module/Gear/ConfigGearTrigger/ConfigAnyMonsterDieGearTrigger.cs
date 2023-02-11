using System;
using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当关卡内怪物死亡")]
    [NinoSerialize]
    public class ConfigAnyMonsterDieGearTrigger : ConfigGearTrigger<AnyMonsterDieEvent>
    {
        
    }
}