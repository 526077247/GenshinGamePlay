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
                CharacterInput.Speed = Quaternion.Inverse(transform.rotation) * animator.velocity;
                // animator.ApplyBuiltinRootMotion();
            }
        }

        private void OnJump(Vector3 v)
        {
			
        }

        private void OnLand(Vector3 v)
        {
			
        }
    }
}