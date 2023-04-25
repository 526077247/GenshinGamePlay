using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoSerialize]
    public partial class SingleSceneGroupValue: BaseSceneGroupValue
    {
        [NinoMember(1)][LabelText("固定值")]
        public int FixedValue;

        public override float Resolve(IEventBase obj, DynDictionary set)
        {
            return FixedValue;
        }
    }
}