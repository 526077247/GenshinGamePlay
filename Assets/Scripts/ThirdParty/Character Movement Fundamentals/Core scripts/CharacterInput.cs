using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
    //This abstract character input class serves as a base for all other character input classes;
    //The 'Controller' component will access this script at runtime to get input for the character's movement (and jumping);
    //By extending this class, it is possible to implement custom character input;
    public abstract class CharacterInput
    {
        public abstract float GetHorizontalMovementInput();
        public abstract float GetVerticalMovementInput();

        public abstract bool IsJumpKeyPressed();
    }
}
