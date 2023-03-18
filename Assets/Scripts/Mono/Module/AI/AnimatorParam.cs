#if UNITY_EDITOR
using UnityEngine;

namespace TaoTie
{
    public class AnimatorParam : StateMachineBehaviour
    {
        [SerializeReference]
        public StateData Data;
    }
}
#endif