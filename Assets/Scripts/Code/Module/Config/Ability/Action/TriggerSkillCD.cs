using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract][Tooltip("使技能进入cd")]
    public partial class TriggerSkillCD: ConfigAbilityAction
    {
        [ProtoMember(11)][LabelText("技能配置表Id")]
        public int ConfigId;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc != null)
            {
                cc.TriggerSkillCD(ConfigId);
            }
        }
    }
}