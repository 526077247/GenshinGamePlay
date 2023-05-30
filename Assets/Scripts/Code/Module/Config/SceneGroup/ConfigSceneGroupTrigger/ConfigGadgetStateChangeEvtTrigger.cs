using Nino.Serialization;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [LabelText("当Gadget状态改变")]
    [NinoSerialize]
    public partial class ConfigGadgetStateChangeEvtTrigger : ConfigSceneGroupTrigger<GadgetStateChangeEvt>
    {
        
    }
}