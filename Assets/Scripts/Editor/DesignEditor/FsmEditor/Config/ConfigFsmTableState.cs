#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    public class ConfigFsmTableState
    {
        public string Name;
        [OnValueChanged(nameof(ChangeName))]
        public AnimationClip Clip;

        public StateData Data = new StateData();

        public void ChangeName()
        {
            if (string.IsNullOrEmpty(Name) && Clip != null)
            {
                Name = Clip.name;
            }
        }
    }
}