namespace TaoTie
{
    public class ConfigExecuteAbility:ConfigFsmClip
    {
        public string AbilityName;
        public override FsmClip CreateClip(FsmState state)
        {
            var res = ExecuteAbilityClip.Create();
            res.OnInit(state,this);
            return res;
        }
    }
}