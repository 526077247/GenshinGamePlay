namespace TaoTie
{
    public class ConfigParamFloat : ConfigParam<float>
    {
        public ConfigParamFloat(){}
        public ConfigParamFloat(string key, float value, bool needSyncAnimator = false) : base(key, value, needSyncAnimator) { }

        public override void SetDefaultValue(DynDictionary dynDictionary)
        {
            if (this.defaultValue != default(float))
            {
                SetValue(dynDictionary, this.defaultValue);
            }
        }
    }
}