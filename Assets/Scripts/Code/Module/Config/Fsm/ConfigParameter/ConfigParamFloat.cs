using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
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