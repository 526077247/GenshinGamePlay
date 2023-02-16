namespace TaoTie
{
    public class ConfigParamTrigger : ConfigParam<bool>
    {
        public ConfigParamTrigger(){}
        public ConfigParamTrigger(string key, bool value, bool needSyncAnimator = false) : base(key, value, needSyncAnimator) { }

        public override void SetDefaultValue(FsmComponent ctrl)
        {
            if (this.defaultValue != default(bool))
            {
                SetValue(ctrl, this.defaultValue);
            }
        }
    }
}