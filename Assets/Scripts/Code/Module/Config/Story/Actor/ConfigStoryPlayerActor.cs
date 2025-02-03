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
                var ghc = avatar?.GetComponent<GameObjectHolderComponent>();
                if (ghc == null) return null;
                await ghc.WaitLoadGameObjectOver();
                if (ghc.IsDispose) return null;
                avatar.RemoveComponent<MoveComponent>();
                return avatar.GetComponent<GameObjectHolderComponent>().EntityView.gameObject;
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