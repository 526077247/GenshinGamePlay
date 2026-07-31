#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    public class GadgetStateChangeEvt: IEventBase
    {
        [SceneGroupActorId]
        [LabelText("GadgetId(及ActorId)")]
        public int GadgetId;
        [LabelText("新状态")]
        public GadgetState State;
        [LabelText("原状态")]
        public GadgetState OldState;
    }
}