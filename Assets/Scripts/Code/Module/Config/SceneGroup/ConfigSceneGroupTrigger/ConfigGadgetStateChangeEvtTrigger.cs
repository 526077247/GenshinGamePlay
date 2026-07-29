using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当Gadget状态改变")]
    [ProtoContract]
    public partial class ConfigGadgetStateChangeEvtTrigger : ConfigSceneGroupTrigger<GadgetStateChangeEvt>
    {
        
    }
}