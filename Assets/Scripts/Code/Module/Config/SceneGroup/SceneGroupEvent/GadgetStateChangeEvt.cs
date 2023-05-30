namespace TaoTie
{
    public class GadgetStateChangeEvt: IEventBase
    {
        [SceneGroupActorId]
        public int GadgetId;
        
        public GadgetState State;
        
        public GadgetState OldState;
    }
}