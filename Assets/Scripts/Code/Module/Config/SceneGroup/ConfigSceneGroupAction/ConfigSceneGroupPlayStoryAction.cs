using Nino.Serialization;
using Sirenix.OdinInspector;

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
        
        protected override void Execute(IEventBase evt, SceneGroup aimSceneGroup, SceneGroup fromSceneGroup)
        {
            aimSceneGroup.Manager.Parent.GetManager<StorySystem>()?.PlayStory(Id,aimSceneGroup.Id).Coroutine();
        }
    }
}