using Nino.Core;
using UnityEngine;

namespace TaoTie
{
    [NinoType(false)]
    public partial class ConfigStoryCharacterActor: ConfigStoryActor
    {
        [NinoMember(5)]
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCharacterConfigIds)+"()")]
#endif
        public int ConfigId;
        [NinoMember(6)]
        public bool Preload3d = true;

        public override async ETTask Preload(StorySystem storySystem)
        {
            var cc = CharacterConfigCategory.Instance.Get(ConfigId);
            var uc = UnitConfigCategory.Instance.Get(cc.UnitId);
            if (Preload3d)
            {
                await GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(uc.Perfab,1);
            }
            else
            {
                GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(uc.Perfab,1).Coroutine();
            }
        }

        public override async ETTask<GameObject> Get3dObj(StorySystem storySystem)
        {
            var cc = CharacterConfigCategory.Instance.Get(ConfigId);
            var uc = UnitConfigCategory.Instance.Get(cc.UnitId);
            return await GameObjectPoolManager.GetInstance().GetGameObjectAsync(uc.Perfab);
        }

        public override void Recycle3dObj(StorySystem storySystem,GameObject obj)
        {
            if (obj != null)
            {
                GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
            }
        }
    }
}