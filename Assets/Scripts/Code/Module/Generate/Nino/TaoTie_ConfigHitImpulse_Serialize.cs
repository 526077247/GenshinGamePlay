/* this is generated by nino */
namespace TaoTie
{
    public partial class ConfigHitImpulse
    {
        [LitJson.Extensions.JsonIgnore]
        public static ConfigHitImpulse.SerializationHelper NinoSerializationHelper = new ConfigHitImpulse.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<ConfigHitImpulse>
        {
            #region NINO_CODEGEN
            public override void Serialize(ConfigHitImpulse value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWriteEnum<TaoTie.HitLevel>(value.HitLevel);
                writer.WriteCommonVal<TaoTie.BaseValue>(value.HitImpulseX==null?TypeInfo<TaoTie.BaseValue>.Type:value.HitImpulseX.GetType(),value.HitImpulseX);
                writer.WriteCommonVal<TaoTie.BaseValue>(value.HitImpulseY==null?TypeInfo<TaoTie.BaseValue>.Type:value.HitImpulseY.GetType(),value.HitImpulseY);
            }

            public override ConfigHitImpulse Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                ConfigHitImpulse value = new ConfigHitImpulse();
                reader.DecompressAndReadEnum<TaoTie.HitLevel>(ref value.HitLevel);
                value.HitImpulseX = reader.ReadCommonVal<TaoTie.BaseValue>();
                value.HitImpulseY = reader.ReadCommonVal<TaoTie.BaseValue>();
                return value;
            }
            #endregion
        }
    }
}