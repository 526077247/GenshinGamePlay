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
	    public Vector3 Speed;
		/// <summary>
		/// 朝向
		/// </summary>
	    public Vector3 Direction;
		/// <summary>
		/// 转向速度(°/s)
		/// </summary>
		public float RolateSpeed = 360;

	    public float GetHorizontalMovementInput()
	    {
		    return 0;
	    }

	    public float GetVerticalMovementInput()
	    {
		    return Speed.z;
	    }

	    public bool IsJumpKeyPressed()
	    {
		    return false;
	    }
    }
}
