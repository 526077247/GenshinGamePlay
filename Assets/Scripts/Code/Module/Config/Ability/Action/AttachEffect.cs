using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 附加特效
    /// </summary>
    [ProtoContract]
    public class AttachEffect: ConfigAbilityAction
    {
        [ProtoMember(10)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetEffects)+"()",AppendNextDrawer = true)]
#endif
        public string EffectName;
        [ProtoMember(11)]
        public ConfigBornType Born;
        [ProtoMember(12)]
        public BaseValue Scale;
        [ProtoMember(13, IsRequired = true)]
        [LabelText("延时销毁(ms)")][Tooltip("-1表示不销毁")]
        public int DelayDestroy = -1;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            ExecuteAsync(actionExecuter,ability,modifier,target).Coroutine();
        }

        protected async ETTask ExecuteAsync(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var pos = Born?.ResolvePos(applier, ability, modifier, target)??Vector3.zero;
            var rot = Born?.ResolveRot(applier, ability, modifier, target)??Quaternion.identity;
            var scale = Scale?.Resolve(target, ability)??1;
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

            if (DelayDestroy >= 0)
            {
                res.DelayDispose(DelayDestroy);
            }

            if (Born != null) await Born.AfterBorn(applier, ability, modifier, target, res);
        }
    }
}