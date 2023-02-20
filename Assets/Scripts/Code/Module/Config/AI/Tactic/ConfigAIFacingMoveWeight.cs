using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveWeight
    {
        private float stopRawNum;
        private float forwardRawNum;
        private float backRawNum;
        private float leftRawNum;
        private float rightRawNum;

    }
}