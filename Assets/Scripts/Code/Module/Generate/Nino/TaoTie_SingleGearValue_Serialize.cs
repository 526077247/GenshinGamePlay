/* this is generated by nino */
namespace TaoTie
{
    public partial class SingleGearValue
    {
        [LitJson.Extensions.JsonIgnore]
        public static SingleGearValue.SerializationHelper NinoSerializationHelper = new SingleGearValue.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<SingleGearValue>
        {
            #region NINO_CODEGEN
            public override void Serialize(SingleGearValue value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWrite(ref value.fixedValue);
            }

            public override SingleGearValue Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                SingleGearValue value = new SingleGearValue();
                reader.DecompressAndReadNumber<System.Int32>(ref value.fixedValue);
                return value;
            }
            #endregion
        }
    }
}