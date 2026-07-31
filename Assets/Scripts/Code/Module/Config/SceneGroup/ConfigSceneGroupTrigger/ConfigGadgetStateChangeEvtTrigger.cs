using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [LabelText("当Gadget状态改变")]
    [ProtoContract]
    public partial class ConfigGadgetStateChangeEvtTrigger : ConfigSceneGroupTrigger<GadgetStateChangeEvt>
    {
        
    }
}