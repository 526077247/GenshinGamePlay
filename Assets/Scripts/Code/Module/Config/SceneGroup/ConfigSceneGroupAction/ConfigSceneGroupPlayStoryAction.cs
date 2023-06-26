using Nino.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 播放剧情
    /// </summary>
    [LabelText("播放剧情")]
    [NinoSerialize]
    public partial class ConfigSceneGroupPlayStoryAction : ConfigSceneGroupAction
    {
        [NinoMember(10)]
        public ulong Id;
        [NinoMember(11)]
        public Vector3 Position;
        [NinoMember(12)]
        public Vector3 Rotation;
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.Manager.Parent.GetManager<StorySystem>()?.PlayStory(Id, aimSceneGroup, Position, Quaternion.Euler(Rotation)).Coroutine();
        }
    }
}