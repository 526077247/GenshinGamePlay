namespace TaoTie
{
    public class ConfigParamBool : ConfigParam<bool>
    {
        public ConfigParamBool(){}
        public ConfigParamBool(string key, bool value, bool needSyncAnimator = false) : base(key, value, needSyncAnimator) { }

        public override void SetDefaultValue(FsmComponent ctrl)
        {
            if (this.defaultValue != default(bool))
            {
                SetValue(ctrl, this.defaultValue);
            }
        }
    }
}