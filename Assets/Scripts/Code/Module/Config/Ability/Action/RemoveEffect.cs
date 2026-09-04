using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    /// <summary>
    /// 移除特效
    /// </summary>
    [ProtoContract]
    public class RemoveEffect: ConfigAbilityAction
    {
        [ProtoMember(10)]
        public string EffectName;

        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var ac = target.GetComponent<AttachComponent>();
            if (ac == null || ac.Childs == null) return;
            for (int i = ac.Childs.Count - 1; i >= 0; i--)
            {
                var childId = ac.Childs[i];
                var child = target.Parent.Get<Entity>(childId);
                if (child is Effect effect && effect.EffectName == EffectName)
                {
                    child.Dispose();
                }
            }
        }
    }
}
