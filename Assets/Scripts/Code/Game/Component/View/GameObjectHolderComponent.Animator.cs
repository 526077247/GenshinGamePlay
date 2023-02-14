using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolderComponent
    {
        public Animator Animator;

        public void SetWeight(int index, float weight)
        {
            if (Animator != null)
            {
                Animator.SetLayerWeight(index, weight);
            }
        }
    }
}