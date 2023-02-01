using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityComponent:Component,IComponent<List<ConfigAbility>>
    {
        private bool isDestroy;
        private ListComponent<ActorAbility> abilities;

        private ListComponent<ActorModifier> moditiers;
        #region override
        public void Init(List<ConfigAbility> data)
        {
            isDestroy = false;
            abilities = ListComponent<ActorAbility>.Create();
            for (int i = 0; i < data.Count; i++)
            {
                var ability = ActorAbility.Create(data[i], this);
                abilities.Add(ability);
                ability.AfterAdd();
            }
        }
		
        public void Destroy()
        {
            isDestroy = true;
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].BeforeRemove();
                abilities[i].Dispose();
            }
            abilities.Dispose();
            
            for (int i = 0; i < moditiers.Count; i++)
            {
                moditiers[i].BeforeRemove();
                moditiers[i].Dispose();
            }
            moditiers.Dispose();
        }

        #endregion


        public void ApplyModifier(long applierID, ActorAbility ability, string modifierName)
        {
            if (!ability.TryGetConfigAbilityModifier(modifierName, out var config))
            {
                Log.Error($"ApplyModifier Fail! modifierName = {modifierName} abilityName = {ability.Config.AbilityName}");
                return;
            }
            var modifier = ActorModifier.Create(applierID, config, ability, this);
            moditiers.Add(modifier);
            modifier.AfterAdd();
        }
        
        public void RemoveModifier(ActorModifier modifier)
        {
            if(isDestroy) return;
            if (moditiers.Contains(modifier))
            {
                modifier.BeforeRemove();
                moditiers.Remove(modifier);
                modifier.Dispose();
            }
        }

        
        
        public void RemoveAbility(ActorAbility ability)
        {
            if(isDestroy) return;
            if (abilities.Contains(ability))
            {
                ability.BeforeRemove();
                abilities.Remove(ability);
                ability.Dispose();
            }
        }
    }
}