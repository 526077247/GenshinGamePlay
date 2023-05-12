using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
	//This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput
    {
	    public MotionFlag MotionFlag;
	    public Vector3 Direction;
        public float GetHorizontalMovementInput()
        {
	        return Direction.x;
        }

		public float GetVerticalMovementInput()
		{
			return Direction.z;
		}

		public bool IsJumpKeyPressed()
		{
			return false;
		}
    }
}
