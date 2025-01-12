using Nino.Core;

namespace TaoTie
{
    [NinoType(false)]
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