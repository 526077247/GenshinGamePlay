using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public class SingleGearValue: BaseGearValue
    {
        [NinoMember(1)][LabelText("固定值")]
        public int fixedValue;

        public override float Resolve(IEventBase obj, VariableSet set)
        {
            return fixedValue;
        }
    }
}