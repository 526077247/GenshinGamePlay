namespace TaoTie
{
    public class ExecuteAbilityAction:FsmClip<ConfigExecuteAbility>
    {
        public static ExecuteAbilityAction Create()
        {
            return ObjectPool.Instance.Fetch<ExecuteAbilityAction>();
        }
        protected override void OnStart()
        {
            actor.GetComponent<AbilityComponent>().ExecuteAbility(config.AbilityName);
        }

        protected override void OnStop()
        {
            
        }

        protected override void OnUpdate(float nowtime, float elapsetime)
        {
            
        }

 
    }
}