using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

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