using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
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