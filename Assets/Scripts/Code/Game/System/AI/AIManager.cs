using System.Collections.Generic;

namespace TaoTie
{
    public class AIManager:IManager<BaseMapScene>,IUpdateManager
    {
        private const int CONST_VALUE_SKILL_CD_MIN_PRESERVE_TIME = 10;
        private BaseMapScene scene;
        private Dictionary<long, AIComponent> aiUnits;
        private LinkedList<AIComponent> allAIUnit;
        private List<AIComponent> localAvatarAlertEnemies;
        private List<AIComponent> localAvatarAwareEnemies;
        private Unit localAvatar;
        private LocalInputController avatarInputController;
        /// <summary>
        /// [campId:[campId:Units]] campId（左）的敌对campId（右）
        /// </summary>
        public UnOrderDoubleKeyDictionary<uint, uint, List<Unit>> aiEnemyEntityTable;
        /// <summary>
        /// [campId:Units]
        /// </summary>
        private UnOrderMultiMap<uint, Unit> configIDEntityTable;
        
        private Dictionary<string, long> publicCDs;
        #region IManager

        public void Init(BaseMapScene mapScene)
        {
            scene = mapScene;
            localAvatar = scene.Self;
            avatarInputController = localAvatar.GetComponent<LocalInputController>();
            configIDEntityTable = new UnOrderMultiMap<uint, Unit>();
            aiEnemyEntityTable = new UnOrderDoubleKeyDictionary<uint, uint, List<Unit>>();
            aiUnits = new Dictionary<long, AIComponent>();
            allAIUnit = new LinkedList<AIComponent>();
            localAvatarAlertEnemies = new List<AIComponent>();
            localAvatarAwareEnemies = new List<AIComponent>();
            
            configIDEntityTable.Add(localAvatar.CampId,localAvatar);
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

        public void AddAI(AIComponent aiComponent)
        {
            aiUnits.Add(aiComponent.Id,aiComponent);
            allAIUnit.AddLast(aiComponent);
            var unit = aiComponent.GetParent<Unit>();
            bool isNew = !configIDEntityTable.ContainsKey(unit.CampId);
            configIDEntityTable.Add(unit.CampId,unit);
            if (isNew)
            {
                foreach (var item in configIDEntityTable)
                {
                    if (item.Key != unit.CampId)//todo:
                    {
                        aiEnemyEntityTable.Add(unit.CampId,item.Key,item.Value);
                        aiEnemyEntityTable.Add(item.Key,unit.CampId,configIDEntityTable[unit.CampId]);
                    }
                }
            }
        }
        
        public void RemoveAI(AIComponent aiComponent)
        {
            var unit = aiComponent.GetParent<Unit>();
            configIDEntityTable.Remove(unit.CampId,unit);
            localAvatarAwareEnemies.Remove(aiComponent);
            localAvatarAlertEnemies.Remove(aiComponent);
            allAIUnit.Remove(aiComponent);
            aiUnits.Remove(aiComponent.Id);
        }

        public bool CanUseSkill(string pCDName, Entity targetEntity)
        {
            if (publicCDs.TryGetValue(pCDName, out var time))
            {
                return GameTimerManager.Instance.GetTimeNow() > time;
            }
            return true;
        }

        public void SetSkillUsed(string pCDName)
        {
            if(!string.IsNullOrEmpty(pCDName))
                publicCDs[pCDName] = GameTimerManager.Instance.GetTimeNow() + CONST_VALUE_SKILL_CD_MIN_PRESERVE_TIME;
        }

        public Dictionary<uint, List<Unit>> GetEnemies(uint campID)
        {
            if(aiEnemyEntityTable.ContainsKey(campID))
                return aiEnemyEntityTable[campID];
            return null;
        }
    }
}