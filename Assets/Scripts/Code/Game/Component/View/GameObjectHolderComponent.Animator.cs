using UnityEngine;

namespace TaoTie
{
    public partial class GameObjectHolderComponent
    {
        public Animator Animator;

        public void MoveStart(Unit unit)
        {
            Animator.CrossFade("Walk",0.1f);
        }
        
        public void MoveStop(Unit unit)
        {
            Animator.CrossFade("Idle",0.1f);
        }
    }
}