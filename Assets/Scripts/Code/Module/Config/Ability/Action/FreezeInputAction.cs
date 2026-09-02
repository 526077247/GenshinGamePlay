using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public class FreezeInputAction : ConfigAbilityAction
    {
        [ProtoMember(10)][LabelText("冻结")]
        public bool Freeze;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var inputController = target.GetComponent<LocalInputController>();
            if (inputController == null) return;
            if (Freeze)
            {
                var moveComponent = target.GetComponent<MoveComponent>();
                var direction = moveComponent?.CharacterInput?.Direction ?? Vector3.zero;
                inputController.FreezeInput(direction);
            }
            else
            {
                
                inputController.UnfreezeInput();
            }
        }
    }
}
