using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{

    public class MoveInput
    {
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
		    return 0;
	    }

	    public float GetVerticalMovementInput()
	    {
		    return Velocity.z;
	    }

	    public bool IsJumpKeyPressed()
	    {
		    return Jump;
	    }
    }
}
