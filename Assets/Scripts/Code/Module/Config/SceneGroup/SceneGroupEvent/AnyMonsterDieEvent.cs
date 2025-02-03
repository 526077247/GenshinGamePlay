using Sirenix.OdinInspector;
namespace TaoTie
{
    public class AnyMonsterDieEvent: IEventBase
    {
        [SceneGroupActorId]
        [LabelText("死亡的单位Id")]
        public int ActorId;
    }
}