using ProtoBuf;
using UnityEngine;
#if UNITY_EDITOR
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
#endif
namespace TaoTie
{
    [ProtoContract]
    [ProtoInclude(100, typeof(ConfigStoryCameraActor))]
    [ProtoInclude(101, typeof(ConfigStoryCharacterActor))]
    [ProtoInclude(102, typeof(ConfigStoryPlayerActor))]
    [ProtoInclude(103, typeof(ConfigStorySceneGroupActor))]
    public abstract partial class ConfigStoryActor
    {
#if UNITY_EDITOR
        [LabelText("策划备注")]
        public string Remarks;
#endif
        [ProtoMember(2)]
        public int Id;

        public virtual async ETTask Preload(StorySystem storySystem)
        {
            await ETTask.CompletedTask;
        }

        public abstract ETTask<GameObject> Get3dObj(StorySystem storySystem);
        
        public virtual void Recycle3dObj(StorySystem storySystem, GameObject obj){}
    }
}