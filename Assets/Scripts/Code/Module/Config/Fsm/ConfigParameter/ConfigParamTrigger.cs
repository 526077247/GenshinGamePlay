using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigParamTrigger : ConfigParam<bool>
    {
        public override void SetDefaultValue(DynDictionary dynDictionary)
        {
            if (this.defaultValue != default(bool))
            {
                SetValue(dynDictionary, this.defaultValue);
            }
        }
    }
}