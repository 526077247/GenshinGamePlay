using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TaoTie
{
    public class SceneGroup : Entity, IEntity<ConfigSceneGroup, SceneGroupManager>
    {
        [Timer(TimerType.GameTimeEventTrigger)]
        public class GameTimeEventTrigger : ATimer<SceneGroup>
        {
            public override void Run(SceneGroup t)
            {
                try
                {
                    Messager.Instance.Broadcast(t.Id, MessageId.SceneGroupEvent, new GameTimeChange()
                    {
                        GameTimeNow = GameTimerManager.Instance.GetTimeNow()
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        public override EntityType Type => EntityType.SceneGroup;
        public Vector3 Position => Config.Position;
        public Vector3 Rotation => Config.Rotation;
        #region IEntity

        public void Init(ConfigSceneGroup p1, SceneGroupManager manager)
        {

            Messager.Instance.AddListener<IEventBase>(Id, MessageId.SceneGroupEvent, OnEvent);
            this.Manager = manager;
            configId = p1.Id;
            Variable = DynDictionary.Create();
            actorEntities = new UnOrderMultiMap<int, long>();
            zoneEntities = new Dictionary<int, long>();
            timerTrigger = new Dictionary<int, long>();
            activeHandlers = new LinkedList<int>();
            AfterLoadFromDB();
            if (Config.RandSuite && Config.Suites != null && Config.Suites.Length > 0)
            {
                this.temp.Clear();
                int total = 0;
                for (int i = 0; i < Config.Suites.Length; i++)
                {
                    this.temp.Add(Config.Suites[i].LocalId, Config.Suites[i].RandWeight);
                    total += Config.Suites[i].RandWeight;
                }

                if (total == 0)
                {
                    Log.Error("随机group失败 totalWeight == 0! sceneGroupId=" + Config.Id);
                    this.ChangeSuite(Config.Suites[0].LocalId);
                    return;
                }

                var flag = Random.Range(0, total * 10) % total;
                foreach (var item in this.temp)
                {
                    if (item.Value > 0)
                    {
                        flag -= item.Value;
                        if (flag <= 0)
                        {
                            this.ChangeSuite(item.Key);
                            break;
                        }
                    }
                }

                Log.Error("随机suite失败 sceneGroupId=" + Config.Id);
                this.ChangeSuite(Config.Suites[0].LocalId);
            }
            else
            {
                this.ChangeSuite(Config.InitSuite);
            }
        }

        public void Destroy()
        {
            this.Manager = null;
            this.triggers = null;
            this.actors = null;
            this.routes = null;
            this.zones = null;
            this.suite = null;
            this.addOnSuiteConfig = null;

            foreach (var item in this.activeEnv)
            {
                EnvironmentManager.Instance?.Remove(item.Value);
            }

            activeEnv = null;

            foreach (var item in this.actorEntities)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    Parent.Remove(item.Value[i]);
                }
            }

            this.actorEntities = null;

            foreach (var item in this.zoneEntities)
            {
                Parent.Remove(item.Value);
            }

            this.zoneEntities = null;

            this.activeHandlers.Clear();
            this.activeHandlers = null;


            this.configId = 0;

            this.Variable.onValueChange -= this.OnVariableChanged;
            this.Variable.Dispose();
            this.Variable = null;
            Messager.Instance.RemoveListener<IEventBase>(Id, MessageId.SceneGroupEvent, OnEvent);
        }

        #endregion

        public SceneGroupManager Manager { get; private set; }
        public DynDictionary Variable { get; set; }
        private ulong configId;

        private UnOrderMultiMap<int, long> actorEntities; // [localid: entityid]

        private Dictionary<int, long> zoneEntities; // [localid: entityid]
        private Dictionary<int, long> timerTrigger; // [localid: entityid]
        private LinkedList<int> activeHandlers;
        private int curSuiteId { get; set; }

        /// <summary>
        /// 附加group
        /// </summary>
        private HashSet<int> addOnSuiteConfig;


        private readonly Dictionary<int, int> temp = new Dictionary<int, int>();

        public ConfigSceneGroup Config => ConfigSceneGroupCategory.Instance.Get(configId);

        private Dictionary<int, ConfigSceneGroupZone> zones;

        private Dictionary<int, ConfigSceneGroupActor> actors;

        private Dictionary<int, ConfigRoute> routes;

        private Dictionary<int, ConfigSceneGroupTrigger> triggers;

        private Dictionary<int, ConfigSceneGroupSuites> suite;
        
        private Dictionary<string, long> activeEnv;


        public ConfigSceneGroupSuites CurGroupSuitesConfig
        {
            get
            {
                if (suite.TryGetValue(curSuiteId, out var res))
                {
                    return res;
                }

                return null;
            }

        }

        private void OnEvent(IEventBase evt)
        {
            if(IsDispose) return;
            for (var node = this.activeHandlers.First; node != null; node = node.Next)
            {
                var trigger = this.triggers[node.Value];
                trigger.OnTrigger(this, evt);
            }
        }

        private void AfterLoadFromDB()
        {
            this.Variable.onValueChange += this.OnVariableChanged;
            this.activeEnv = new Dictionary<string, long>();
            this.actors = new Dictionary<int, ConfigSceneGroupActor>(this.Config.Actors?.Length??0);
            this.routes = new Dictionary<int, ConfigRoute>(this.Config.Route?.Length??0);
            this.triggers = new Dictionary<int, ConfigSceneGroupTrigger>(this.Config.Triggers?.Length??0);
            this.zones = new Dictionary<int, ConfigSceneGroupZone>(this.Config.Zones?.Length??0);
            this.suite = new Dictionary<int, ConfigSceneGroupSuites>(this.Config.Suites?.Length??0);

            for (int i = 0; i < (this.Config.Actors?.Length??0); i++)
            {
                this.actors.Add(this.Config.Actors[i].LocalId, this.Config.Actors[i]);
            }

            for (int i = 0; i < (this.Config.Route?.Length??0); i++)
            {
                this.routes.Add(this.Config.Route[i].LocalId, this.Config.Route[i]);
            }

            for (int i = 0; i < (this.Config.Triggers?.Length??0); i++)
            {
                this.triggers.Add(this.Config.Triggers[i].LocalId, this.Config.Triggers[i]);
            }

            for (int i = 0; i < (this.Config.Zones?.Length??0); i++)
            {
                this.zones.Add(this.Config.Zones[i].LocalId, this.Config.Zones[i]);
            }

            for (int i = 0; i < (this.Config.Suites?.Length??0); i++)
            {
                this.suite.Add(this.Config.Suites[i].LocalId, this.Config.Suites[i]);
            }
        }

        private void OnVariableChanged(string key, float newVal, float oldVal)
        {
            Messager.Instance.Broadcast(Id, MessageId.SceneGroupEvent, new VariableChangeEvent()
            {
                Key = key,
                NewValue = newVal,
                OldValue = oldVal,
            });
        }

        #region 切换Suite

        public void ChangeSuite(int suiteId)
        {
            //先把附加的移除
            if (this.addOnSuiteConfig != null)
            {
                foreach (var item in this.addOnSuiteConfig)
                {
                    this.RemoveExtraSuite(item);
                }
            }

            // 新的
            if (this.suite.TryGetValue(suiteId, out var config))
            {
                this.ChangeTriggers(config);
                this.ChangeZones(config);
                this.ChangeActors(config);
                this.curSuiteId = config.LocalId;
                Messager.Instance.Broadcast(Id, MessageId.SceneGroupEvent, new SuiteLoadEvent()
                {
                    SuiteId = curSuiteId,
                    IsAddOn = false,
                });
            }
        }

        private void Collect(int[] pre, int[] next)
        {
            this.temp.Clear();
            // 旧的
            if (pre != null)
            {
                for (int i = 0; i < pre.Length; i++)
                {
                    if (!this.temp.ContainsKey(pre[i]))
                    {
                        this.temp.Add(pre[i], -1);
                    }
                    else
                    {
                        this.temp[pre[i]]--;
                    }

                }
            }

            // 新的
            if (next != null)
            {
                for (int i = 0; i < next.Length; i++)
                {
                    if (!this.temp.ContainsKey(next[i]))
                    {
                        this.temp.Add(next[i], 1);
                    }
                    else
                    {
                        this.temp[next[i]]++;
                    }
                }
            }

        }

        private void ChangeActors(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.Actors : null, config.Actors);
            foreach (var item in this.temp)
            {
                if (item.Value > 0) //新增actor
                {
                    if (!this.actorEntities.ContainsKey(item.Key) && this.actors.TryGetValue(item.Key, out var actor))
                    {
                        var unit = actor.CreateActor(this, 0);
                        this.actorEntities.Add(item.Key, unit.Id);
                    }
                }
                else if (item.Value < 0) //消失actor
                {
                    if (this.actorEntities.TryGetValue(item.Key, out var entityIds))
                    {
                        for (int i = 0; i < entityIds.Count; i++)
                        {
                            Parent.Remove(entityIds[i]);
                        }
                    }
                }
            }
        }

        private void ChangeTriggers(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.Triggers : null,
                config.Triggers);

            foreach (var item in this.temp)
            {
                if (item.Value > 0) //新增
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this.activeHandlers.AddLast(trigger.LocalId);
                        if (trigger is ConfigGameTimeChangeTrigger timeEventTrigger)
                        {
                            var timerId = GameTimerManager.Instance.NewOnceTimer(timeEventTrigger.GameTime,
                                TimerType.GameTimeEventTrigger, this);
                            this.timerTrigger.Add(trigger.LocalId, timerId);
                        }
                    }
                }
                else if (item.Value < 0) //消失
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this.activeHandlers.Remove(trigger.LocalId);
                        if (trigger is ConfigGameTimeChangeTrigger &&
                            this.timerTrigger.TryGetValue(trigger.LocalId, out var timerId))
                        {
                            GameTimerManager.Instance.Remove(ref timerId);
                            this.timerTrigger.Remove(trigger.LocalId);
                        }
                    }
                }
            }
        }

        private void ChangeZones(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.Zones : null, config.Zones);

            foreach (var item in this.temp)
            {
                if (item.Value > 0) //新增
                {
                    if (!this.zoneEntities.ContainsKey(item.Key) && this.zones.TryGetValue(item.Key, out var zone))
                    {
                        var unit = zone.CreateZone(this);
                        this.zoneEntities[zone.LocalId] = unit.Id;
                    }
                }
                else if (item.Value < 0) //消失
                {
                    if (this.zoneEntities.TryGetValue(item.Key, out var zone))
                    {
                        Parent.Remove(zone);
                    }
                }
            }
        }

        #endregion

        #region 附加Suite

        /// <summary>
        /// 附加group
        /// </summary>
        /// <param name="suiteId"></param>
        public void AddExtraSuite(int suiteId)
        {
            if (this.CurGroupSuitesConfig == null) return;

            // 新的
            if (suiteId != this.curSuiteId && this.suite.TryGetValue(suiteId, out var config)
                                           && (this.addOnSuiteConfig == null ||
                                               !this.addOnSuiteConfig.Contains(config.LocalId)))
            {
                if (this.addOnSuiteConfig == null)
                {
                    this.addOnSuiteConfig = new HashSet<int>();
                }

                this.AddonTriggers(config);
                this.AddonZones(config);
                this.AddonActors(config);
                this.addOnSuiteConfig.Add(config.LocalId);
                Messager.Instance.Broadcast(Id, MessageId.SceneGroupEvent, new SuiteLoadEvent()
                {
                    SuiteId = curSuiteId,
                    IsAddOn = true,
                });
            }
        }

        private void AddonActors(ConfigSceneGroupSuites config)
        {
            if (config.Actors == null) return;
            for (int i = 0; i < config.Actors.Length; i++)
            {
                if (!this.actorEntities.ContainsKey(config.Actors[i]) &&
                    this.actors.TryGetValue(config.Actors[i], out var actor))
                {
                    var unit = actor.CreateActor(this, 0);
                    this.actorEntities.Add(actor.LocalId, unit.Id);
                }
            }
        }

        private void AddonTriggers(ConfigSceneGroupSuites config)
        {
            if (config.Triggers == null) return;
            for (int i = 0; i < config.Triggers.Length; i++)
            {
                if (this.triggers.TryGetValue(config.Triggers[i], out var trigger) &&
                    !this.activeHandlers.Contains(trigger.LocalId))
                {
                    this.activeHandlers.AddLast(trigger.LocalId);
                }
            }
        }

        private void AddonZones(ConfigSceneGroupSuites config)
        {
            if (config.Zones == null) return;
            for (int i = 0; i < config.Zones.Length; i++)
            {
                if (!this.zoneEntities.ContainsKey(config.Zones[i]) &&
                    this.zones.TryGetValue(config.Zones[i], out var zone))
                {
                    var unit = zone.CreateZone(this);
                    this.zoneEntities[zone.LocalId] = unit.Id;
                }
            }

        }

        /// <summary>
        /// 移除附加group
        /// </summary>
        /// <param name="suiteId"></param>
        public void RemoveExtraSuite(int suiteId)
        {
            if (this.suite.TryGetValue(suiteId, out var config) && this.addOnSuiteConfig != null &&
                this.addOnSuiteConfig.Contains(config.LocalId))
            {
                this.RemoveAddonActors(config);
                this.RemoveAddonZones(config);
                this.RemoveAddonTriggers(config);
                this.addOnSuiteConfig.Remove(config.LocalId);
            }
        }

        private void Collect(List<int[]> pre, int[] next)
        {
            this.temp.Clear();
            // 旧的
            if (pre != null)
            {
                for (int i = 0; i < pre.Count; i++)
                {
                    if (pre[i] == null) continue;
                    for (int j = 0; j < pre[i].Length; j++)
                    {
                        if (!this.temp.ContainsKey(pre[i][j]))
                        {
                            this.temp.Add(pre[i][j], +1);
                        }
                        else
                        {
                            this.temp[pre[i][j]]++;
                        }
                    }
                }
            }

            // 新的
            if (next != null)
            {
                for (int i = 0; i < next.Length; i++)
                {
                    if (!this.temp.ContainsKey(next[i]))
                    {
                        this.temp.Add(next[i], -1);
                    }
                    else
                    {
                        this.temp[next[i]]--;
                    }
                }
            }

        }

        private void RemoveAddonActors(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.Actors);
            foreach (var item in this.addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.Actors);
            }

            this.Collect(pre, config.Actors);
            foreach (var item in this.temp)
            {
                if (item.Value <= 0) //消失actor
                {
                    if (this.actorEntities.TryGetValue(item.Key, out var entityIds))
                    {
                        for (int i = 0; i < entityIds.Count; i++)
                        {
                            Parent.Remove(entityIds[i]);
                        }
                    }
                }
            }
        }

        private void RemoveAddonTriggers(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.Triggers);
            foreach (var item in this.addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.Triggers);
            }

            this.Collect(pre, config.Triggers);
            foreach (var item in this.temp)
            {
                if (item.Value <= 0) //消失
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this.activeHandlers.Remove(trigger.LocalId);
                    }
                }
            }
        }

        private void RemoveAddonZones(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.Zones);
            foreach (var item in this.addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.Zones);
            }

            this.Collect(pre, config.Zones);
            foreach (var item in this.temp)
            {
                if (item.Value <= 0) //消失
                {
                    if (this.zoneEntities.TryGetValue(item.Key, out var zone))
                    {
                        Parent.Remove(zone);
                    }
                }
            }
        }

        #endregion

        #region 数据

        public bool TryGetRoute(int routeId, out ConfigRoute route)
        {
            return this.routes.TryGetValue(routeId, out route);
        }

        /// <summary>
        /// 根据actorId取，默认取第一个
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool TryGetActorEntity(int actorId, out long entityId)
        {
            if (this.actorEntities.TryGetValue(actorId, out var entityIds))
            {
                if (entityIds.Count > 0)
                {
                    entityId = entityIds[0];
                    return true;
                }
            }
            entityId = 0;
            return false;
        }
        public bool TryGetZoneEntity(int actorId, out long entityId)
        {
            return this.zoneEntities.TryGetValue(actorId, out entityId);
        }
        /// <summary>
        /// 从SceneGroup的管理中移除，不会销毁Entity
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="unitId"></param>
        public void OnActorRelease(int actorId, long unitId)
        {
            if (this.actorEntities.TryGetValue(actorId, out var entityIds))
            {
                entityIds.Remove(unitId);
                if (entityIds.Count <= 0)
                {
                    this.actorEntities.Remove(actorId);
                }
            }
        }

        /// <summary>
        /// 创建Actor
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="range"></param>
        public void CreateActor(int actorId, float range = 0)
        {
            if (this.actors.TryGetValue(actorId, out var actor))
            {
                var unit = actor.CreateActor(this, range);
                this.actorEntities.Add(actor.LocalId, unit.Id);
            }
        }

        /// <summary>
        /// 获取存活怪物数量
        /// </summary>
        /// <returns></returns>
        public int GetSuiteMonsterCount()
        {
            int res = 0;
            if (this.Config.Actors != null)
            {
                for (int i = 0; i < this.Config.Actors.Length; i++)
                {
                    if (this.Config.Actors[i] is ConfigSceneGroupActorMonster monster &&
                        this.actorEntities.TryGetValue(monster.LocalId, out var eid))
                    {
                        res += eid.Count;
                    }
                }
            }
            return res;
        }

        #endregion

        #region 环境
        
        public void PushEnvironment(int envId, EnvironmentPriorityType type, string key)
        {
            if (key != null && !activeEnv.ContainsKey(key))
            {
                var id = EnvironmentManager.Instance.Create(envId, type);
                if (id != 0) activeEnv.Add(key, id);
            }
        }
        
        public void RemoveEnvironment(string key)
        {
            if (key != null && activeEnv.TryGetValue(key, out var id))
            {
                EnvironmentManager.Instance.Remove(id);
                activeEnv.Remove(key);
            }
        }

        #endregion
    }
}