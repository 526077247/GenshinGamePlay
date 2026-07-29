using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
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