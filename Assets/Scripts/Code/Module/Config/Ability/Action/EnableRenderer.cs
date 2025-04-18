﻿using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class EnableRenderer: ConfigAbilityAction
    {
        [NinoMember(10)]
        public bool SetEnable;
        [NinoMember(11)]
        public bool IncludeChild;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            TryEnableRenderer(target);
        }

        private void TryEnableRenderer(Entity target)
        {
            if(target == null || target.IsDispose) return;
            UnitModelComponent unitModelComponent = target.GetComponent<UnitModelComponent>();
            unitModelComponent?.EnableRenderer(SetEnable).Coroutine();
            EffectModelComponent effectModelComponent = target.GetComponent<EffectModelComponent>();
            effectModelComponent?.EnableRenderer(SetEnable).Coroutine();
            if (IncludeChild)
            {
                var ac = target.GetComponent<AttachComponent>();
                if (ac != null)
                {
                    for (int i = 0; i < ac.Childs.Count; i++)
                    {
                        var child = target.Parent.Get<Entity>(ac.Childs[i]);
                        TryEnableRenderer(child);
                    }
                }
            }
        }
    }
}