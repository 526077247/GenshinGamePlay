#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
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