using UnityEngine;

namespace TaoTie
{
    public partial class MoveComponent
    {
        private void OnAnimatorMove()
        {
            var animator = GetComponent<GameObjectHolderComponent>()?.Animator;
            if (animator != null)
            {
                animator.ApplyBuiltinRootMotion();
            }
        }
    }
}