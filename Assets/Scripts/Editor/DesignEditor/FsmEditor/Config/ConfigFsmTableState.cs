using Sirenix.OdinInspector;
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