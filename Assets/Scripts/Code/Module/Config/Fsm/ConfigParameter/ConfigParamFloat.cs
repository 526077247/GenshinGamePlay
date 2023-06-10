using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public class ConfigParamFloat : ConfigParam<float>
    {

        public override void SetDefaultValue(DynDictionary dynDictionary)
        {
            if (this.defaultValue != default(float))
            {
                SetValue(dynDictionary, this.defaultValue);
            }
        }
    }
}