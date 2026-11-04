using ProtoBuf;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TaoTie.Inspector;
#endif

namespace TaoTie
{
    [ProtoContract]
    public partial class ConfigSkill
    {
        [ProtoMember(1)] [LabelText("默认技能（填 SkillConfig.Id）")]
        public int[] Skills;

        /// <summary>冲刺技能（<see cref="SkillConfig.Id"/>）。0 = 这个角色没有冲刺。</summary>
        [ProtoMember(2)] [LabelText("冲刺（SkillConfig.Id，0=无）")] 
        public int SprintBS;
    }
}