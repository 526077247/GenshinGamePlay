namespace TaoTie
{
    public class ConfigParamFloat : ConfigParam<float>
    {
        public ConfigParamFloat(){}
        public ConfigParamFloat(string key, float value, bool needSyncAnimator = false) : base(key, value, needSyncAnimator) { }

        public override void SetDefaultValue(FsmComponent ctrl)
        {
            if (this.defaultValue != default(float))
            {
                SetValue(ctrl, this.defaultValue);
            }
        }
    }
}