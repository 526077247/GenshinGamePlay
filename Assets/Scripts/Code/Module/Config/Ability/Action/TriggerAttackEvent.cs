using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class TriggerAttackEvent : ConfigAbilityAction
    {
        [NinoMember(10)]
        public TargetType TargetType;
        [NotNull] [NinoMember(11)]
        public ConfigAttackEvent AttackEvent;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (TargetType.None == TargetType || AttackEvent == null || AttackEvent.AttackInfo == null ||
                AttackEvent.AttackPattern == null) return;
            var len = AttackEvent.AttackPattern.ResolveHit(applier, ability, modifier, target,
                new[] {EntityType.ALL}, out var infos);
            for (int i = 0; i < len; i++)
            {
                var info = infos[i];
                var hitEntity = target.Parent.Get<Entity>(info.EntityId);
                if (TargetType == TargetType.Self && info.EntityId != target.Id)
                    continue;
                if (TargetType == TargetType.AllExceptSelf && info.EntityId == target.Id)
                    continue;
                if (TargetType == TargetType.Enemy && !AttackHelper.CheckIsEnemy(target, hitEntity))
                    continue;
                if (TargetType == TargetType.SelfCamp && !AttackHelper.CheckIsCamp(target, hitEntity))
                    continue;
                AttackResult result = AttackResult.Create(target.Id, hitEntity.Id, info, AttackEvent.AttackInfo);
                AttackHelper.DamageClose(ability, modifier, result);
                result.Dispose();
            }
        }
    }
}