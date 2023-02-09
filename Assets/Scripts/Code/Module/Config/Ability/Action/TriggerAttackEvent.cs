namespace TaoTie
{
    public class TriggerAttackEvent : ConfigAbilityAction
    {
        public TargetType TargetType;
        public ConfigAttackEvent AttackEvent;

        protected override void Execute(Entity applier, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            if (TargetType.None == TargetType) return;
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