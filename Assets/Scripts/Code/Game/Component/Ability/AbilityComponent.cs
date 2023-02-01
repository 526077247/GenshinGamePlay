using System.Collections.Generic;

namespace TaoTie
{
    public class AbilityComponent:Component,IComponent<List<ConfigAbility>>
    {
        private ListComponent<Ability> abilities;

        #region override
        public void Init(List<ConfigAbility> data)
        {
            abilities = ListComponent<Ability>.Create();
            for (int i = 0; i < data.Count; i++)
            {
                var ability = Ability.Create(data[i], this);
                abilities.Add(ability);
                ability.AfterAdd();
            }
        }
		
        public void Destroy()
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].BeforeRemove();
                abilities[i].Dispose();
            }
            abilities.Dispose();
        }

        #endregion
    }
}