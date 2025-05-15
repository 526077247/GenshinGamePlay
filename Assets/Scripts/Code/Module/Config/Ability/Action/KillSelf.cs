using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class KillSelf: ConfigAbilityAction
    {
        [NinoMember(10)]
        public DieStateFlag DieFlag;
        [NinoMember(11)][LabelText("*Gadget是否下一帧销毁")][Tooltip("注意时序，先Kill了可能会影响后面执行的Action内部的判断，所以一般开启此项")]
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