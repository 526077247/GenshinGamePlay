using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigParamBool : ConfigParam<bool>
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