using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize]
    public partial class ConfigAIFacingMoveData
    {
        private int speedLevelRawNum;
        private float rangeMinRawNum;
        private float rangeMaxRawNum;
        private float restTimeMinRawNum;
        private float restTimeMaxRawNum;
        private float facingMoveTurnIntervalRawNum;
        private float facingMoveMinAvoidanceVelecityRawNum;
        private float obstacleDetectRangeRawNum;
        private ConfigAIFacingMoveWeight facingMoveWeight; 
    }
}