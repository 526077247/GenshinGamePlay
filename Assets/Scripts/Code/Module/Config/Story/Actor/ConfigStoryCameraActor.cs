using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigStoryCameraActor: ConfigStoryActor
    {
        [NinoMember(10)] 
        public int CameraConfigId = 2;

        private long id;
        public override async ETTask<GameObject> Get3dObj(StorySystem storySystem)
        {
            await ETTask.CompletedTask;
            CameraManager.Instance.Remove(ref id);
            id = CameraManager.Instance.Create(CameraConfigId, 999);
            return CameraManager.Instance.MainCamera().gameObject;
        }

        public override void Recycle3dObj(StorySystem storySystem,GameObject obj)
        {
            CameraManager.Instance.Remove(ref id);
            obj.transform.SetParent(null);
        }
    }
}