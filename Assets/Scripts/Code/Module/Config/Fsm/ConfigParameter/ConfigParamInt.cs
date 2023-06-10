using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigParamInt : ConfigParam<int>
    {

        public override void SetDefaultValue(DynDictionary dynDictionary)
        {
            if (this.defaultValue != default(int))
            {
                SetValue(dynDictionary, this.defaultValue);
            }
        }
    }
}