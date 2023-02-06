namespace TaoTie
{
    public class AfterAttackMixin: AbilityMixin
    {
        public class AttackInfo
        {
            public Entity attacker;
            public Entity beAttacker;
            public int num;
        }
        public ConfigAfterAttackMixin config => baseConfig as ConfigAfterAttackMixin;
        
        public override void Init(ActorAbility actorAbility, ActorModifier actorModifier, ConfigAbilityMixin config)
        {
            base.Init(actorAbility, actorModifier, config);
            Messager.Instance.AddListener<AttackInfo>(actorAbility.Parent.GetParent<Entity>().Id,MessageId.Attack,Excute);
        }

        public override void Dispose()
        {
            Messager.Instance.RemoveListener<AttackInfo>(actorAbility.Parent.GetParent<Entity>().Id,MessageId.Attack,Excute);
            base.Dispose();
        }

        public void Excute(AttackInfo attackInfo)
        {
            if (config.Actions != null)
            {
                for (int i = 0; i < config.Actions.Length; i++)
                {
                    config.Actions[i].DoExecute(actorAbility.Parent.GetParent<Entity>(), actorAbility, actorModifier, attackInfo.beAttacker);
                }
            }
        }
    }
}