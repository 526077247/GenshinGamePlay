using System.Collections.Generic;

namespace TaoTie
{
    public class AIManager:IManager<MapScene>,IUpdate
    {
        private const int CONST_VALUE_SKILL_CD_MIN_PRESERVE_TIME = 10;
        private MapScene scene;
        private Dictionary<long, AIComponent> unitIdUnits;
        private LinkedList<AIComponent> allAIUnit;
        private List<AIComponent> localAvatarAlertEnemies;
        private List<AIComponent> localAvatarAwareEnemies;
        private Actor localAvatar;
        private LocalInputController avatarInputController;
        /// <summary>
        /// [campId:[campId:Units]] campId（左）的敌对campId（右）
        /// </summary>
        public UnOrderDoubleKeyDictionary<uint, uint, List<Actor>> campIdCampIdEntityTable;
        /// <summary>
        /// [campId:Units]
        /// </summary>
        private UnOrderMultiMap<uint, Actor> campIdEntityTable;
        
        private Dictionary<string, long> publicCDs;
        #region IManager

        public void Init(MapScene mapScene)
        {
            scene = mapScene;
            localAvatar = scene.Self;
            avatarInputController = localAvatar.GetComponent<LocalInputController>();
            campIdEntityTable = new UnOrderMultiMap<uint, Actor>();
            campIdCampIdEntityTable = new UnOrderDoubleKeyDictionary<uint, uint, List<Actor>>();
            unitIdUnits = new Dictionary<long, AIComponent>();
            allAIUnit = new LinkedList<AIComponent>();
            localAvatarAlertEnemies = new List<AIComponent>();
            localAvatarAwareEnemies = new List<AIComponent>();
            publicCDs = new Dictionary<string, long>();
            
            campIdEntityTable.Add(localAvatar.CampId,localAvatar);
            Messager.Instance.AddListener<Actor>(0,MessageId.OnBeKill,Remove);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<Actor>(0,MessageId.OnBeKill,Remove);
            localAvatarAwareEnemies = null;
            localAvatarAlertEnemies = null;
            allAIUnit = null;
            unitIdUnits = null;
            campIdEntityTable = null;
            campIdCampIdEntityTable = null;
            publicCDs = null;
            avatarInputController = null;
            localAvatar = null;
            scene = null;
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
            unitIdUnits.Add(aiComponent.Id,aiComponent);
            allAIUnit.AddLast(aiComponent);
            var unit = aiComponent.GetParent<Actor>();
            bool isNew = !campIdEntityTable.ContainsKey(unit.CampId);
            campIdEntityTable.Add(unit.CampId,unit);
            if (isNew)
            {
                foreach (var item in campIdEntityTable)
                {
                    if (AttackHelper.CheckIsEnemyCamp(item.Key,unit.CampId))
                    {
                        campIdCampIdEntityTable.Add(unit.CampId,item.Key,item.Value);
                        campIdCampIdEntityTable.Add(item.Key,unit.CampId,campIdEntityTable[unit.CampId]);
                    }
                }
            }
        }
        
        public void RemoveAI(AIComponent aiComponent)
        {
            var unit = aiComponent.GetParent<Actor>();
            campIdEntityTable.Remove(unit.CampId,unit);
            localAvatarAwareEnemies.Remove(aiComponent);
            localAvatarAlertEnemies.Remove(aiComponent);
            allAIUnit.Remove(aiComponent);
            unitIdUnits.Remove(aiComponent.Id);
        }
        
        private void Remove(Actor unit)
        {
            if (unitIdUnits.TryGetValue(unit.Id, out var aiComponent))
            {
                localAvatarAwareEnemies.Remove(aiComponent);
                localAvatarAlertEnemies.Remove(aiComponent);
                allAIUnit.Remove(aiComponent);
                unitIdUnits.Remove(aiComponent.Id);
            }
            campIdEntityTable.Remove(unit.CampId,unit);
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

        public Dictionary<uint, List<Actor>> GetEnemies(uint campID)
        {
            if(campIdCampIdEntityTable.ContainsKey(campID))
                return campIdCampIdEntityTable[campID];
            return null;
        }

        public Unit GetUnit(long id)
        {
            if (id == scene.MyId)
            {
                return scene.Self;
            }
            if (unitIdUnits.TryGetValue(id, out var res))
            {
                return res.GetParent<Unit>();
            }

            return null;
        }
    }
}