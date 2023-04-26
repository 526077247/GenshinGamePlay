using Nino.Serialization;

namespace TaoTie
{
    [NinoSerialize()]
    public partial class ConfigTrackCamera: ConfigCamera
    {
        public override CameraType Type => CameraType.TrackCameraPlugin;

        [NinoMember(20)]
        public ConfigTrackedDolly TrackedDolly;
    }
}