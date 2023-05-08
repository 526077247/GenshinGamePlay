using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {
	    public Vector3 Direction;
        public override float GetHorizontalMovementInput()
        {
	        return Direction.x;
        }

		public override float GetVerticalMovementInput()
		{
			return Direction.z;
		}

		public override bool IsJumpKeyPressed()
		{
			return false;
		}
    }
}
