#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class AnimatorParam : StateMachineBehaviour
    {
        [SerializeReference][HideReferenceObjectPicker]
        public StateData Data = new StateData();
    }
}
#endif