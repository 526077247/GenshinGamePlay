/* this is generated by nino */
namespace TaoTie
{
    public partial class ByAnd
    {
        [LitJson.Extensions.JsonIgnore]
        public static ByAnd.SerializationHelper NinoSerializationHelper = new ByAnd.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<ByAnd>
        {
            #region NINO_CODEGEN
            public override void Serialize(ByAnd value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWriteEnum<TaoTie.AbilityTargetting>(value.Target);
                writer.Write(value.Predicates);
            }

            public override ByAnd Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                ByAnd value = new ByAnd();
                reader.DecompressAndReadEnum<TaoTie.AbilityTargetting>(ref value.Target);
                value.Predicates = reader.ReadArray<TaoTie.ConfigAbilityPredicate>();
                return value;
            }
            #endregion
        }
    }
}