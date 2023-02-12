namespace TaoTie
{
    public struct AnyMonsterDieEvent: IEventBase
    {
        [GearActorId]
        public int ActorId;
    }
}