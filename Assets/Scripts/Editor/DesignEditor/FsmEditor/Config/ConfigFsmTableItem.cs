using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class ConfigFsmTableItem
    {
        [ReadOnly][ShowIf("@"+nameof(FromState)+"!="+nameof(ToState))]
        public string FromState;
        [ReadOnly]
        public string ToState;
        [OnCollectionChanged(nameof(Refresh))]
        public ConfigTransition[] Transitions;

        private void Refresh()
        {
            for (int i = 0; i < (Transitions?.Length??0); i++)
            {
                if (Transitions[i] != null)
                {
                    Transitions[i].FromState = FromState;
                    Transitions[i].ToState = ToState;
                }
            }
        }
    }
}