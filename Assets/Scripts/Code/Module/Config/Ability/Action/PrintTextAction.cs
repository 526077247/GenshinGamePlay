using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif
using UnityEngine;

namespace TaoTie
{
    [ProtoContract]
    public partial class PrintTextAction: ConfigAbilityAction
    {
        /// <summary>
        /// 打印的文本
        /// </summary>
        [ProtoMember(1)]
        public string Text;
        protected override void Execute(Entity actionExecuter, ActorAbility ability, ActorModifier modifier, Entity target)
        {
            Debug.Log(Text);
        }
    }
}