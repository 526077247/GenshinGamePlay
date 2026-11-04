using ProtoBuf;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
namespace TaoTie
{
    [ProtoContract]
    public partial class AddSkillInfo: ConfigAbilityAction
    {
        /// <summary>技能配置表Id（<see cref="SkillConfig.Id"/>）。不带槽位 —— 运行时加的技能没有按键入口。</summary>
        [ProtoMember(10)]
        public int ConfigId;
        [ProtoMember(11)][LabelText("槽位号(0表示不绑定)")][Range(0, GameConst.SkillSlotConst)]
        public int Slot;
        
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            var cc = target.GetComponent<SkillComponent>();
            if (cc != null && ConfigId != 0)
            {
                if (Slot == 0)
                {
                    cc.AddSkillInfo(ConfigId);  
                }
                else
                {
                    cc.SetSkillInfo(ConfigId, Slot);
                }
            }
        }
    }
}