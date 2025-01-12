using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public abstract partial class ConfigStoryActor
    {
#if UNITY_EDITOR
        [Sirenix.OdinInspector.LabelText("策划备注")]
        public string Remarks;
#endif
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