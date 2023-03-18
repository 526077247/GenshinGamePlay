using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// 显示伤害飘字
    /// </summary>
    [NinoSerialize]
    public partial class ConfigShowFightText : ConfigAttackResult
    {
        public override void ResolveAttackResult(AttackResult attackResult)
        {
            Debug.Log("zzz final_dmg" + attackResult.FinalRealDamage + attackResult.HitInfo.HitPos);
            var hudView = UIManager.Instance.GetWindow<UIHudView>();
            if (hudView != null)
            {
                 hudView.ShowFightText(attackResult);
            }
        }
    }
}