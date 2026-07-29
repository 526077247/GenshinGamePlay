using ProtoBuf;
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigStoryCameraActor: ConfigStoryActor
    {
        [ProtoMember(10, IsRequired = true)] 
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