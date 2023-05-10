#if UNITY_EDITOR
using Slate;

namespace TaoTie
{
    [Attachable(typeof(ActorGroup))]
    public class SpineAnimatorTrack: CutsceneTrack
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.name = "Spine动画轨道";
        }
    }
}
#endif