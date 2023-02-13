#if UNITY_EDITOR
using Slate;

namespace TaoTie
{
    [Attachable(typeof(FSMTrack))]
    public class ApplyModifierClip: ActionClip
    {
        public string ModifierName;
        public string AbilityName;
    }
}
#endif