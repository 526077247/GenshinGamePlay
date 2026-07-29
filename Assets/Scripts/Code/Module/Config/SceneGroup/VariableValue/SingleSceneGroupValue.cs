using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract]
    public partial class SingleSceneGroupValue: BaseSceneGroupValue
    {
        [ProtoMember(1)][LabelText("固定值")]
        public int FixedValue;

        public override float Resolve(IEventBase obj, DynDictionary set)
        {
            return FixedValue;
        }
    }
}