using System.Collections.Generic;

namespace TaoTie
{
    public class AIManager:IManager,IUpdateComponent
    {

        private Dictionary<long, AIComponent> aiUnits;
        private LinkedList<AIComponent> allAIUnit;
        private List<AIComponent> localAvatarAlertEnemies;
        private List<AIComponent> localAvatarAwareEnemies;
        
        private Dictionary<string, PublicAISkillCD> publicCDs;
        #region IManager

        public void Init()
        {
            aiUnits = new Dictionary<long, AIComponent>();
            allAIUnit = new LinkedList<AIComponent>();
            localAvatarAlertEnemies = new List<AIComponent>();
            localAvatarAwareEnemies = new List<AIComponent>();
        }

        public void Destroy()
        {
            localAvatarAwareEnemies = null;
            localAvatarAlertEnemies = null;
            allAIUnit = null;
            aiUnits = null;
        }

        public void Update()
        {
            for (var i = allAIUnit.First; i !=null; i=i.Next)
            {
                i.Value.Update();
            }
        }

        #endregion

        public void AddAI(AIComponent unit)
        {
            aiUnits.Add(unit.Id,unit);
            allAIUnit.AddLast(unit);
        }
        
        public void RemoveAI(AIComponent unit)
        {
            localAvatarAwareEnemies.Remove(unit);
            localAvatarAlertEnemies.Remove(unit);
            allAIUnit.Remove(unit);
            aiUnits.Remove(unit.Id);
        }

        public bool CanUseSkill(string pCDName, Entity targetEntity)
        {
            return true;
        }

        public void SetSkillUsed(string pCDName)
        {
            
        } 
    }
}