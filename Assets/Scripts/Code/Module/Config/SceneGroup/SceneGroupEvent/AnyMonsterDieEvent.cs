namespace TaoTie
{
    public struct AnyMonsterDieEvent: IEventBase
    {
        [SceneGroupActorId]
        public int ActorId;
    }
}