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
    public partial class KillSelf: ConfigAbilityAction
    {
        [ProtoMember(10)]
        public DieStateFlag DieFlag;
        [ProtoMember(11, IsRequired = true)][LabelText("*Gadget是否下一帧销毁")][Tooltip("注意时序，先Kill了可能会影响后面执行的Action内部的判断，所以一般开启此项")]
        public bool KillNextFrame = true;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<CombatComponent>();
            if (cc != null)
            {
                target.GetComponent<CombatComponent>().DoKill(actionExecuter.Id, DieFlag);
            }
            else
            {
                if(KillNextFrame)
                    target.DelayDispose(1);
                else
                    target.Dispose();
            }
        }
    }
}