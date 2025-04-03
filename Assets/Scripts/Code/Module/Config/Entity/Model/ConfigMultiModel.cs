using Nino.Core;
using Sirenix.OdinInspector;

namespace TaoTie
{
    [NinoType(false)][LabelText("多实例")]
    public partial class ConfigMultiModel: ConfigModel
    {
        [NinoMember(1)][LabelText("数量")]
        public BaseValue Count = new NumericValue();
        [NinoMember(2)][LabelText("中心点偏移")] 
        public DynamicVector3 Offset = new DynamicVector3();
        [NinoMember(3)][LabelText("排列方式")]
        public ConfigArrange Arrange;
    }
}