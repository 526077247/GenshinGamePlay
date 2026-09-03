#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    public class AnimatorParam : SerializedStateMachineBehaviour
    {
        [SerializeReference]
#if UNITY_EDITOR
        [HideReferenceObjectPicker]
#endif
        public StateData Data = new StateData();
    }
}