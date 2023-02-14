/* this is generated by nino */
namespace TaoTie
{
    public partial class OperatorGearValue
    {
        [LitJson.Extensions.JsonIgnore]
        public static OperatorGearValue.SerializationHelper NinoSerializationHelper = new OperatorGearValue.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<OperatorGearValue>
        {
            #region NINO_CODEGEN
            public override void Serialize(OperatorGearValue value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.WriteCommonVal<TaoTie.BaseGearValue>(value.value1==null?TypeInfo<TaoTie.BaseGearValue>.Type:value.value1.GetType(),value.value1);
                writer.CompressAndWriteEnum<TaoTie.LogicMode>(value._op);
                writer.WriteCommonVal<TaoTie.BaseGearValue>(value.value2==null?TypeInfo<TaoTie.BaseGearValue>.Type:value.value2.GetType(),value.value2);
            }

            public override OperatorGearValue Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                OperatorGearValue value = new OperatorGearValue();
                value.value1 = reader.ReadCommonVal<TaoTie.BaseGearValue>();
                reader.DecompressAndReadEnum<TaoTie.LogicMode>(ref value._op);
                value.value2 = reader.ReadCommonVal<TaoTie.BaseGearValue>();
                return value;
            }
            #endregion
        }
    }
}