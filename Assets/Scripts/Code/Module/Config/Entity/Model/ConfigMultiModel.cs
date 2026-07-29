using ProtoBuf;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [ProtoContract][LabelText("多实例")]
    public partial class ConfigMultiModel: ConfigModel
    {
        [ProtoMember(1, IsRequired = true)][LabelText("数量")]
        public BaseValue Count = new NumericValue();
        [ProtoMember(2, IsRequired = true)][LabelText("中心点偏移")] [NotNull]
        public DynamicVector3 Offset = new DynamicVector3();
        [ProtoMember(3)][LabelText("排列方式")]
        public ConfigArrange Arrange;
    }
}