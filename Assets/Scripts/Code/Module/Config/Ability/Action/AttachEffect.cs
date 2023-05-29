using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 附加特效
    /// </summary>
    [NinoSerialize]
    public class AttachEffect: ConfigAbilityAction
    {
        [NinoMember(10)]
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetEffects)+"()",AppendNextDrawer = true)]
        public string EffectName;
        [NinoMember(11)]
        public ConfigBornType Born;
        [NinoMember(12)]
        public BaseValue Scale;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ExecuteAsync(applier,ability,modifier,target).Coroutine();
        }

        protected async ETTask ExecuteAsync(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = Born.ResolvePos(applier, ability, modifier, target);
            var rot = Born.ResolveRot(applier, ability, modifier, target);
            var scale = Scale.Resolve(target, ability);
            var res = target.Parent.CreateEntity<Effect, string>(EffectName);
           
            res.Position = pos;
            res.Rotation = rot;
            var count = AbilityHelper.ResolveTarget(applier, ability, modifier, target, AbilityTargetting.Target, out var entities);
            if (count > 0)
            {
                var owner = entities[0];
                //todo: sightGroupWithOwner
                owner.GetOrAddComponent<AttachComponent>().AddChild(res);
            }
            
            await Born.AfterBorn(applier, ability, modifier, target,res);
            
            var goh = res.GetComponent<GameObjectHolderComponent>();
            await goh.WaitLoadGameObjectOver();
            if(goh.IsDispose) return;
            goh.EntityView.localScale = Vector3.one * scale;
        }
    }
}