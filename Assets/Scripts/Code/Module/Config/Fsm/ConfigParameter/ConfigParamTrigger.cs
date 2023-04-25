using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigParamTrigger : ConfigParam<bool>
    {
        public ConfigParamTrigger(){}
        public ConfigParamTrigger(string key, bool value, bool needSyncAnimator = false) : base(key, value, needSyncAnimator) { }

        public override void SetDefaultValue(DynDictionary dynDictionary)
        {
            if (this.defaultValue != default(bool))
            {
                SetValue(dynDictionary, this.defaultValue);
            }
        }
    }
}