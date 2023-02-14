/* this is generated by nino */
namespace TaoTie
{
    public partial class ConfigExitZoneEventTrigger
    {
        [LitJson.Extensions.JsonIgnore]
        public static ConfigExitZoneEventTrigger.SerializationHelper NinoSerializationHelper = new ConfigExitZoneEventTrigger.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<ConfigExitZoneEventTrigger>
        {
            #region NINO_CODEGEN
            public override void Serialize(ConfigExitZoneEventTrigger value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWrite(ref value.localId);
                writer.Write(value.actions);
            }

            public override ConfigExitZoneEventTrigger Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                ConfigExitZoneEventTrigger value = new ConfigExitZoneEventTrigger();
                reader.DecompressAndReadNumber<System.Int32>(ref value.localId);
                value.actions = reader.ReadArray<TaoTie.ConfigGearAction>();
                return value;
            }
            #endregion
        }
    }
}