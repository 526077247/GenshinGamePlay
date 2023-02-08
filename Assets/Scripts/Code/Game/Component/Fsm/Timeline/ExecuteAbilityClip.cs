namespace TaoTie
{
    public class ExecuteAbilityClip:FsmClip<ConfigExecuteAbility>
    {
        public static ExecuteAbilityClip Create()
        {
            return ObjectPool.Instance.Fetch<ExecuteAbilityClip>();
        }
        protected override void OnStart()
        {
            _actor.GetComponent<AbilityComponent>().ExecuteAbility(config.AbilityName);
        }

        protected override void OnStop()
        {
            
        }

        protected override void OnUpdate(float nowtime, float elapsetime)
        {
            
        }

 
    }
}