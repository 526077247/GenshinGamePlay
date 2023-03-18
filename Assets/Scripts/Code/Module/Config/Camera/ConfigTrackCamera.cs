using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize()]
    public partial class ConfigTrackCamera: ConfigCamera
    {
        public override CameraType type => CameraType.TrackCameraPlugin;

        [NinoMember(20)]
        public ConfigTrackedDolly trackedDolly;
    }
}