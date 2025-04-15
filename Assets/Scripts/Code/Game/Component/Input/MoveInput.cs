using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{

    public class MoveInput
    {
	    public MotionDirection MotionDirection;
		/// <summary>
		/// 速度
		/// </summary>
	    public Vector3 Velocity;
		/// <summary>
		/// 朝向
		/// </summary>
	    public Vector3 Direction;
		/// <summary>
		/// 速度比例
		/// </summary>
		public float SpeedScale = 1;
		/// <summary>
		/// 转向速度(°/s)
		/// </summary>
		public float RolateSpeed = 360;

		public bool Jump = false;

	    public float GetHorizontalMovementInput()
	    {
		    if (MotionDirection == MotionDirection.Left)
		    {
			    return -Velocity.magnitude;
		    }
		    else if (MotionDirection == MotionDirection.Right)
		    {
			    return Velocity.magnitude;
		    }
		    return 0;
	    }

	    public float GetVerticalMovementInput()
	    {
		    if (MotionDirection == MotionDirection.Left || MotionDirection == MotionDirection.Right) return 0;
		    return Velocity.z;
	    }

	    public bool IsJumpKeyPressed()
	    {
		    return Jump;
	    }
    }
}
