using UnityEngine;

namespace TaoTie
{
    public class AvatarMoveComponent : Component, IComponent
    {
        private NumericComponent NumericComponent => Parent.GetComponent<NumericComponent>();
        public void Init()
        {

        }

        public void Destroy()
        {

        }

        public void TryMove(Vector3 direction)
        {
            GetParent<Unit>().Position += (GameTimerManager.Instance.GetDeltaTime() / 1000f) * NumericComponent.GetAsFloat(NumericType.Speed) * direction;
        }
    }
}
