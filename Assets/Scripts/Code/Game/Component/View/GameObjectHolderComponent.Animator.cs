using CMF;
using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolderComponent
    {
        private Animator animator;
        private FsmComponent fsm => parent.GetComponent<FsmComponent>();
        public void SetWeight(int index, float weight)
        {
            if (animator != null)
            {
                animator.SetLayerWeight(index, weight);
            }
        }

        private void UpdateMotionFlag(AIMoveSpeedLevel level)
        {
            fsm.SetData(FSMConst.MotionFlag,(int)level);
        }

        private void SetData(string key, int data)
        {
            if (animator == null) return;
            animator.SetInteger(key, data);
        }

        private void SetData(string key, float data)
        {
            if (animator == null) return;
            animator.SetFloat(key, data);
        }

        private void SetData(string key, bool data)
        {
            if (animator == null) return;
            animator.SetBool(key, data);
        }

        private void CrossFadeInFixedTime(string stateName, float fadeDuration, int layerIndex, float targetTime)
        {
            if (animator == null) return;
            animator.CrossFadeInFixedTime(stateName, fadeDuration, layerIndex, targetTime);
        }

        private void CrossFade(string stateName, int layerIndex)
        {
            if (animator == null) return;
            animator.CrossFade(stateName, 0, layerIndex);
        }
        
        // Controller controller;

        //Whether the character is using the strafing blend tree;
		public bool useStrafeAnimations = false;

		//Velocity threshold for landing animation;
		//Animation will only be triggered if downward velocity exceeds this threshold;
		public float landVelocityThreshold = 5f;

		private float smoothingFactor = 40f;
		Vector3 oldMovementVelocity = Vector3.zero;
		
		public void Update () 
		{
		// 	if(controller==null) return;
		// 	//Get controller velocity;
		// 	Vector3 _velocity = controller.GetVelocity();
		//
		// 	//Split up velocity;
		// 	Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, EntityView.up);
		// 	Vector3 _verticalVelocity = _velocity - _horizontalVelocity;
		//
		// 	//Smooth horizontal velocity for fluid animation;
		// 	_horizontalVelocity = Vector3.Lerp(oldMovementVelocity, _horizontalVelocity, smoothingFactor * Time.deltaTime);
		// 	oldMovementVelocity = _horizontalVelocity;
		//
		// 	animator.SetFloat("VerticalSpeed", _verticalVelocity.magnitude * VectorMath.GetDotProduct(_verticalVelocity.normalized, EntityView.up));
		// 	animator.SetFloat("HorizontalSpeed", _horizontalVelocity.magnitude);
		//
		// 	//If animator is strafing, split up horizontal velocity;
		// 	if(useStrafeAnimations)
		// 	{
		// 		Vector3 _localVelocity = EntityView.InverseTransformVector(_horizontalVelocity);
		// 		animator.SetFloat("ForwardSpeed", _localVelocity.z);
		// 		animator.SetFloat("StrafeSpeed", _localVelocity.x);
		// 	}
		//
		// 	//Pass values to animator;
		// 	animator.SetBool("IsGrounded", controller.IsGrounded());
		// 	animator.SetBool("IsStrafing", useStrafeAnimations);
		}
    }
}