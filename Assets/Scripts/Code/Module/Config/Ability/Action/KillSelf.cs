using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)][Tooltip("注意时序，先KillSelf了后面可能执行的Action会影响判断")]
    public partial class KillSelf: ConfigAbilityAction
    {
        [NinoMember(10)]
        public DieStateFlag DieFlag;
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<CombatComponent>();
            if (cc != null)
            {
                target.GetComponent<CombatComponent>().DoKill(actionExecuter.Id, DieFlag);
            }
            else
            {
                target.Dispose();
            }
        }
    }
}