using ProtoBuf;

namespace TaoTie
{
    /// <summary>
    /// 取Ability值
    /// </summary>
    [ProtoContract]
    public partial class AbilityValue: BaseValue
    {
        [ProtoMember(1)]
        public string Key;
        public override float Resolve(Entity entity,ActorAbility ability)
        {
            if(ability != null)
                return ability.GetReplaceValue(Key);
            Log.Error("不支持取Ability值");
            return 0;
        }
    }
}