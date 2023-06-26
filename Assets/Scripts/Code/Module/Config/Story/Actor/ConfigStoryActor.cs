using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    
    public abstract partial class ConfigStoryActor
    {
        [NinoMember(1)][Sirenix.OdinInspector.LabelText("策划备注")]
        public string Remarks;
        [NinoMember(2)]
        public int Id;

        public virtual async ETTask Preload(StorySystem storySystem)
        {
            await ETTask.CompletedTask;
        }

        public abstract ETTask<GameObject> Get3dObj(StorySystem storySystem);
        
        public virtual void Recycle3dObj(GameObject obj){}
    }
}