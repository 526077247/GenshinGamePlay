﻿using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class KillSelf: ConfigAbilityAction
    {
        [NinoMember(10)]
        public DieStateFlag DieFlag;
        
        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<CombatComponent>();
            if (cc != null)
            {
                target.GetComponent<CombatComponent>().DoKill(applier.Id, DieFlag);
            }
            else
            {
                target.Dispose();
            }
        }
    }
}