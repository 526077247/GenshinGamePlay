using Slate;

namespace TaoTie
{
    [Attachable(typeof(ActorGroup))]
    public class FSMTrack: CutsceneTrack
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.name = "状态机轨道";
        }
    }
}