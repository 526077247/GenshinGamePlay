using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
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
                await GameObjectPoolManager.Instance.PreLoadGameObjectAsync(uc.Perfab,1);
            }
            else
            {
                GameObjectPoolManager.Instance.PreLoadGameObjectAsync(uc.Perfab,1).Coroutine();
            }
        }

        public override async ETTask<GameObject> Get3dObj(StorySystem storySystem)
        {
            var cc = CharacterConfigCategory.Instance.Get(ConfigId);
            var uc = UnitConfigCategory.Instance.Get(cc.UnitId);
            return await GameObjectPoolManager.Instance.GetGameObjectAsync(uc.Perfab);
        }

        public override void Recycle3dObj(GameObject obj)
        {
            if (obj != null)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
            }
        }
    }
}