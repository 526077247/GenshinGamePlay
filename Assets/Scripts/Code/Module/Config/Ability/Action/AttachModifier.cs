﻿using Nino.Core;

namespace TaoTie
{
    /// <summary>
    /// 附加Modifier，会随被附加的移除而移除
    /// </summary>
    [NinoType(false)]
    public partial class AttachModifier: ConfigAbilityAction
    {
        [NinoMember(10)]
        public string ModifierName;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AbilityComponent>();
            if (ac != null)
            {
                var newModifier = ac.ApplyModifier(actionExecuter.Id, ability, ModifierName);
                if (modifier != null)
                {
                    modifier.AddAttachedModifer(newModifier);
                }
                else
                {
                    ability.AddAttachedModifer(newModifier);
                }
            }
        }
    }
}