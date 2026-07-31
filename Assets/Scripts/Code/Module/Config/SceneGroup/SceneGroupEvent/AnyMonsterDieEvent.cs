#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    public class AnyMonsterDieEvent: IEventBase
    {
        [SceneGroupActorId]
        [LabelText("死亡的单位Id")]
        public int ActorId;
    }
}