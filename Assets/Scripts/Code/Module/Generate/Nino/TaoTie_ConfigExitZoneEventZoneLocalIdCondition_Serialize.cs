/* this is generated by nino */
namespace TaoTie
{
    public partial class ConfigExitZoneEventZoneLocalIdCondition
    {
        [LitJson.Extensions.JsonIgnore]
        public static ConfigExitZoneEventZoneLocalIdCondition.SerializationHelper NinoSerializationHelper = new ConfigExitZoneEventZoneLocalIdCondition.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<ConfigExitZoneEventZoneLocalIdCondition>
        {
            #region NINO_CODEGEN
            public override void Serialize(ConfigExitZoneEventZoneLocalIdCondition value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWriteEnum<TaoTie.CompareMode>(value.mode);
                writer.CompressAndWrite(ref value.value);
            }

            public override ConfigExitZoneEventZoneLocalIdCondition Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                ConfigExitZoneEventZoneLocalIdCondition value = new ConfigExitZoneEventZoneLocalIdCondition();
                reader.DecompressAndReadEnum<TaoTie.CompareMode>(ref value.mode);
                reader.DecompressAndReadNumber<System.Int32>(ref value.value);
                return value;
            }
            #endregion
        }
    }
}