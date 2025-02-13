#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace TaoTie
{
    public class AnimatorParam : StateMachineBehaviour
    {
        [SerializeReference]
#if UNITY_EDITOR
        [HideReferenceObjectPicker]
#endif
        public StateData Data = new StateData();
    }
}