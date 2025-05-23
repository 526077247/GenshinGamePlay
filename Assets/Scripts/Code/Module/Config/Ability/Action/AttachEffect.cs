﻿using Nino.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 附加特效
    /// </summary>
    [NinoType(false)]
    public class AttachEffect: ConfigAbilityAction
    {
        [NinoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetEffects)+"()",AppendNextDrawer = true)]
#endif
        public string EffectName;
        [NinoMember(11)]
        public ConfigBornType Born;
        [NinoMember(12)]
        public BaseValue Scale;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ExecuteAsync(actionExecuter,ability,modifier,target).Coroutine();
        }

        protected async ETTask ExecuteAsync(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            var rot = Born.ResolveRot(applier, ability, modifier, target);
            var scale = Scale.Resolve(target, ability);
            var res = target.Parent.CreateEntity<Effect, string>(EffectName);
           
            res.Position = pos;
            res.Rotation = rot;
            res.LocalScale = Vector3.one * scale;
            using (var entities = TargetHelper.ResolveTarget(applier, ability, modifier, target, AbilityTargetting.Target))
            {
                if (entities.Count > 0)
                {
                    var owner = entities[0];
                    //todo: sightGroupWithOwner
                    owner.GetOrAddComponent<AttachComponent>().AddChild(res);
                }
            }
            await Born.AfterBorn(applier, ability, modifier, target,res);
        }
    }
}