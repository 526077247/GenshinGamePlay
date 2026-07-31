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
    public partial class ConfigStoryCharacterActor: ConfigStoryActor
    {
        [ProtoMember(5)]
#if UNITY_EDITOR
        [ValueDropdown("@"+nameof(OdinDropdownHelper)+"."+nameof(OdinDropdownHelper.GetCharacterConfigIds)+"()")]
#endif
        public int ConfigId;
        [ProtoMember(6, IsRequired = true)]
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