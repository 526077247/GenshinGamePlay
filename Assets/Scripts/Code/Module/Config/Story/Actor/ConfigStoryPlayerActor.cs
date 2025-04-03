using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigStoryPlayerActor: ConfigStoryActor
    {
        public override async ETTask<GameObject> Get3dObj(StorySystem storySystem)
        {
            if (storySystem.Scene is MapScene scene)
            {
                var avatar = scene.Self as Avatar;
                var model = avatar?.GetComponent<ModelComponent>();
                if (model == null) return null;
                await model.WaitLoadGameObjectOver();
                if (model.IsDispose) return null;
                avatar.RemoveComponent<MoveComponent>();
                return avatar.GetComponent<ModelComponent>().EntityView.gameObject;
            }
            return null;
        }

        public override void Recycle3dObj(StorySystem storySystem,GameObject obj)
        {
            if (storySystem.Scene is MapScene scene)
            {
                var avatar = scene.Self as Avatar;
                avatar?.AddComponent<MoveComponent>();
                var root = scene.GetManager<EntityManager>()?.GameObjectRoot;
                if(root!=null) obj.transform.SetParent(root);
            }

            base.Recycle3dObj(storySystem,obj);
        }
    }
}