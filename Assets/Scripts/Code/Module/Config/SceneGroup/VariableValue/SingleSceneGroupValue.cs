using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)]
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