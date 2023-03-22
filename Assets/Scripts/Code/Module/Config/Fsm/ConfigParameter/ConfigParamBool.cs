namespace TaoTie
{
    public class ConfigParamBool : ConfigParam<bool>
    {
        public ConfigParamBool(){}
        public ConfigParamBool(string key, bool value, bool needSyncAnimator = false) : base(key, value, needSyncAnimator) { }

        public override void SetDefaultValue(DynDictionary dynDictionary)
        {
            if (this.defaultValue != default(bool))
            {
                SetValue(dynDictionary, this.defaultValue);
            }
        }
    }
}