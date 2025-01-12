using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigStorySceneGroupActor: ConfigStoryActor
    {
        [NinoMember(5)]
        public int LocalId;


        public override async ETTask<GameObject> Get3dObj(StorySystem storySystem)
        {
            await ETTask.CompletedTask;
            if (storySystem.SceneGroup.TryGetActorEntity(LocalId, out var unit))
            {
                
            }

            return null;
        }
    }
}