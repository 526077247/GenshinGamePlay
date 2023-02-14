/* this is generated by nino */
namespace TaoTie
{
    public partial class RemoveModifier
    {
        [LitJson.Extensions.JsonIgnore]
        public static RemoveModifier.SerializationHelper NinoSerializationHelper = new RemoveModifier.SerializationHelper();
        public class SerializationHelper: Nino.Serialization.NinoWrapperBase<RemoveModifier>
        {
            #region NINO_CODEGEN
            public override void Serialize(RemoveModifier value, Nino.Serialization.Writer writer)
            {
                if(value == null)
                {
                    writer.Write(false);
                    return;
                }
                writer.Write(true);
                writer.CompressAndWriteEnum<TaoTie.AbilityTargetting>(value.Targetting);
                writer.WriteCommonVal<TaoTie.ConfigSelectTargets>(value.OtherTargets==null?TypeInfo<TaoTie.ConfigSelectTargets>.Type:value.OtherTargets.GetType(),value.OtherTargets);
                writer.WriteCommonVal<TaoTie.ConfigAbilityPredicate>(value.Predicate==null?TypeInfo<TaoTie.ConfigAbilityPredicate>.Type:value.Predicate.GetType(),value.Predicate);
                writer.WriteCommonVal<TaoTie.ConfigAbilityPredicate>(value.PredicateForeach==null?TypeInfo<TaoTie.ConfigAbilityPredicate>.Type:value.PredicateForeach.GetType(),value.PredicateForeach);
                writer.Write(value.ModifierName);
            }

            public override RemoveModifier Deserialize(Nino.Serialization.Reader reader)
            {
                if(!reader.ReadBool())
                    return null;
                RemoveModifier value = new RemoveModifier();
                reader.DecompressAndReadEnum<TaoTie.AbilityTargetting>(ref value.Targetting);
                value.OtherTargets = reader.ReadCommonVal<TaoTie.ConfigSelectTargets>();
                value.Predicate = reader.ReadCommonVal<TaoTie.ConfigAbilityPredicate>();
                value.PredicateForeach = reader.ReadCommonVal<TaoTie.ConfigAbilityPredicate>();
                value.ModifierName = reader.ReadString();
                return value;
            }
            #endregion
        }
    }
}