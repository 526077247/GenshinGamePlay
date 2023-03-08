using System;
using System.Collections.Generic;
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
                    Messager.Instance.Broadcast(t.Id, MessageId.GameTimeEventTrigger, new GameTimeChange()
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

        #region IEntity

        public void Init(ConfigSceneGroup p1, SceneGroupManager manager)
        {

            Messager.Instance.AddListener<IEventBase>(Id, MessageId.SceneGroupEvent, OnEvent);
            this.manager = manager;
            configId = p1.id;
            variable = VariableSet.Create();
            _actorEntities = new Dictionary<int, long>();
            _zoneEntities = new Dictionary<int, long>();
            _timerTrigger = new Dictionary<int, long>();
            _activeHandlers = new LinkedList<int>();
            AfterLoadFromDB();
            if (config.randSuite && config.suites != null && config.suites.Length > 0)
            {
                this._temp.Clear();
                int total = 0;
                for (int i = 0; i < config.suites.Length; i++)
                {
                    this._temp.Add(config.suites[i].localId, config.suites[i].randWeight);
                    total += config.suites[i].randWeight;
                }

                if (total == 0)
                {
                    Log.Error("随机group失败 totalWeight == 0! sceneGroupId=" + config.id);
                    this.ChangeSuite(config.suites[0].localId);
                    return;
                }

                var flag = Random.Range(0, total * 10) % total;
                foreach (var item in this._temp)
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

                Log.Error("随机suite失败 sceneGroupId=" + config.id);
                this.ChangeSuite(config.suites[0].localId);
            }
            else
            {
                this.ChangeSuite(config.initSuite);
            }
        }

        public void Destroy()
        {
            this.manager = null;
            this.triggers = null;
            this.actors = null;
            this.routes = null;
            this.zones = null;
            this.suite = null;
            this._addOnSuiteConfig = null;

            foreach (var item in this._actorEntities)
            {
                Parent.Remove(item.Value);
            }

            this._actorEntities = null;

            foreach (var item in this._zoneEntities)
            {
                Parent.Remove(item.Value);
            }

            this._zoneEntities = null;

            this._activeHandlers.Clear();
            this._activeHandlers = null;


            this.configId = 0;

            this.variable.onValueChange -= this.OnVariableChanged;
            this.variable.Dispose();
            this.variable = null;
            Messager.Instance.RemoveListener<IEventBase>(Id, MessageId.SceneGroupEvent, OnEvent);
        }

        #endregion

        public SceneGroupManager manager { get; private set; }
        public VariableSet variable { get; set; }
        public ulong configId;

        public Dictionary<int, long> _actorEntities; // [localid: entityid]

        public Dictionary<int, long> _zoneEntities; // [localid: entityid]
        public Dictionary<int, long> _timerTrigger; // [localid: entityid]
        public LinkedList<int> _activeHandlers;
        public int curSuiteId { get; set; }

        /// <summary>
        /// 附加group
        /// </summary>
        public HashSet<int> _addOnSuiteConfig;


        public readonly Dictionary<int, int> _temp = new Dictionary<int, int>();

        public ConfigSceneGroup config => ConfigSceneGroupCategory.Instance.Get(configId);

        public Dictionary<int, ConfigSceneGroupZone> zones { get; set; }

        public Dictionary<int, ConfigSceneGroupActor> actors { get; set; }

        public Dictionary<int, ConfigRoute> routes { get; set; }

        public Dictionary<int, ConfigSceneGroupTrigger> triggers { get; set; }

        public Dictionary<int, ConfigSceneGroupSuites> suite { get; set; }


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
            for (var node = this._activeHandlers.First; node != null; node = node.Next)
            {
                var trigger = this.triggers[node.Value];
                trigger.OnTrigger(this, evt);
            }
        }

        private void AfterLoadFromDB()
        {
            this.variable.onValueChange += this.OnVariableChanged;

            this.actors = new Dictionary<int, ConfigSceneGroupActor>(this.config.actors.Length);
            this.routes = new Dictionary<int, ConfigRoute>(this.config.route != null ? this.config.route.Length : 0);
            this.triggers = new Dictionary<int, ConfigSceneGroupTrigger>(this.config.triggers.Length);
            this.zones = new Dictionary<int, ConfigSceneGroupZone>(this.config.zones.Length);
            this.suite = new Dictionary<int, ConfigSceneGroupSuites>(this.config.suites.Length);

            for (int i = 0; i < (this.config.actors == null ? 0 : this.config.actors.Length); i++)
            {
                this.actors.Add(this.config.actors[i].localId, this.config.actors[i]);
            }

            for (int i = 0; i < (this.config.route == null ? 0 : this.config.route.Length); i++)
            {
                this.routes.Add(this.config.route[i].localId, this.config.route[i]);
            }

            for (int i = 0; i < (this.config.triggers == null ? 0 : this.config.triggers.Length); i++)
            {
                this.triggers.Add(this.config.triggers[i].localId, this.config.triggers[i]);
            }

            for (int i = 0; i < (this.config.zones == null ? 0 : this.config.zones.Length); i++)
            {
                this.zones.Add(this.config.zones[i].localId, this.config.zones[i]);
            }

            for (int i = 0; i < (this.config.suites == null ? 0 : this.config.suites.Length); i++)
            {
                this.suite.Add(this.config.suites[i].localId, this.config.suites[i]);
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
            if (this._addOnSuiteConfig != null)
            {
                foreach (var item in this._addOnSuiteConfig)
                {
                    this.RemoveExtraGroup(item);
                }
            }

            // 新的
            if (this.suite.TryGetValue(suiteId, out var config))
            {
                this.ChangeTriggers(config);
                this.ChangeActors(config);
                this.ChangeZones(config);
                this.curSuiteId = config.localId;
                Messager.Instance.Broadcast(Id, MessageId.SceneGroupEvent, new SuiteLoadEvent()
                {
                    SuiteId = curSuiteId,
                    IsAddOn = false,
                });
            }
        }

        private void Collect(int[] pre, int[] next)
        {
            this._temp.Clear();
            // 旧的
            if (pre != null)
            {
                for (int i = 0; i < pre.Length; i++)
                {
                    if (!this._temp.ContainsKey(pre[i]))
                    {
                        this._temp.Add(pre[i], -1);
                    }
                    else
                    {
                        this._temp[pre[i]]--;
                    }

                }
            }

            // 新的
            if (next != null)
            {
                for (int i = 0; i < next.Length; i++)
                {
                    if (!this._temp.ContainsKey(next[i]))
                    {
                        this._temp.Add(next[i], 1);
                    }
                    else
                    {
                        this._temp[next[i]]++;
                    }
                }
            }

        }

        private void ChangeActors(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.actors : null, config.actors);
            foreach (var item in this._temp)
            {
                if (item.Value > 0) //新增actor
                {
                    if (!this._actorEntities.ContainsKey(item.Key) && this.actors.TryGetValue(item.Key, out var actor))
                    {
                        var unit = actor.CreateActor(this);
                        this._actorEntities.Add(item.Key, unit.Id);
                    }
                }
                else if (item.Value < 0) //消失actor
                {
                    if (this._actorEntities.TryGetValue(item.Key, out long entityId))
                    {
                        Parent.Remove(entityId);
                    }
                }
            }
        }

        private void ChangeTriggers(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.triggers : null,
                config.triggers);

            foreach (var item in this._temp)
            {
                if (item.Value > 0) //新增
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this._activeHandlers.AddLast(trigger.localId);
                        if (trigger is ConfigGameTimeEventTrigger timeEventTrigger)
                        {
                            var timerId = GameTimerManager.Instance.NewOnceTimer(timeEventTrigger.GameTime,
                                TimerType.GameTimeEventTrigger, this);
                            this._timerTrigger.Add(trigger.localId, timerId);
                        }
                    }
                }
                else if (item.Value < 0) //消失
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this._activeHandlers.Remove(trigger.localId);
                        if (trigger is ConfigGameTimeEventTrigger &&
                            this._timerTrigger.TryGetValue(trigger.localId, out var timerId))
                        {
                            GameTimerManager.Instance.Remove(ref timerId);
                            this._timerTrigger.Remove(trigger.localId);
                        }
                    }
                }
            }
        }

        private void ChangeZones(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.zones : null, config.zones);

            foreach (var item in this._temp)
            {
                if (item.Value > 0) //新增
                {
                    if (!this._zoneEntities.ContainsKey(item.Key) && this.zones.TryGetValue(item.Key, out var zone))
                    {
                        var unit = zone.CreateZone(this);
                        this._zoneEntities[zone.localId] = unit.Id;
                    }
                }
                else if (item.Value < 0) //消失
                {
                    if (this._zoneEntities.TryGetValue(item.Key, out var zone))
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
        public void AddExtraGroup(int suiteId)
        {
            if (this.CurGroupSuitesConfig == null) return;

            // 新的
            if (suiteId != this.curSuiteId && this.suite.TryGetValue(suiteId, out var config)
                                           && (this._addOnSuiteConfig == null ||
                                               !this._addOnSuiteConfig.Contains(config.localId)))
            {
                if (this._addOnSuiteConfig == null)
                {
                    this._addOnSuiteConfig = new HashSet<int>();
                }

                this.AddonTriggers(config);
                this.AddonActors(config);
                this.AddonZones(config);
                this._addOnSuiteConfig.Add(config.localId);
                Messager.Instance.Broadcast(Id, MessageId.SceneGroupEvent, new SuiteLoadEvent()
                {
                    SuiteId = curSuiteId,
                    IsAddOn = true,
                });
            }
        }

        private void AddonActors(ConfigSceneGroupSuites config)
        {
            for (int i = 0; i < config.actors.Length; i++)
            {
                if (!this._actorEntities.ContainsKey(config.actors[i]) &&
                    this.actors.TryGetValue(config.actors[i], out var actor))
                {
                    var unit = actor.CreateActor(this);
                    this._actorEntities.Add(actor.localId, unit.Id);
                }
            }
        }

        private void AddonTriggers(ConfigSceneGroupSuites config)
        {
            for (int i = 0; i < config.triggers.Length; i++)
            {
                if (this.triggers.TryGetValue(config.triggers[i], out var trigger) &&
                    !this._activeHandlers.Contains(trigger.localId))
                {
                    this._activeHandlers.AddLast(trigger.localId);
                }
            }
        }

        private void AddonZones(ConfigSceneGroupSuites config)
        {
            for (int i = 0; i < config.zones.Length; i++)
            {
                if (!this._zoneEntities.ContainsKey(config.zones[i]) &&
                    this.zones.TryGetValue(config.zones[i], out var zone))
                {
                    var unit = zone.CreateZone(this);
                    this._zoneEntities[zone.localId] = unit.Id;
                }
            }

        }

        /// <summary>
        /// 移除附加group
        /// </summary>
        /// <param name="group"></param>
        public void RemoveExtraGroup(int group)
        {
            if (this.suite.TryGetValue(group, out var config) && this._addOnSuiteConfig != null &&
                this._addOnSuiteConfig.Contains(config.localId))
            {
                this.RemoveAddonZones(config);
                this.RemoveAddonActors(config);
                this.RemoveAddonTriggers(config);
                this._addOnSuiteConfig.Remove(config.localId);
            }
        }

        private void Collect(List<int[]> pre, int[] next)
        {
            this._temp.Clear();
            // 旧的
            if (pre != null)
            {
                for (int i = 0; i < pre.Count; i++)
                {
                    for (int j = 0; j < pre[i].Length; j++)
                    {
                        if (!this._temp.ContainsKey(pre[i][j]))
                        {
                            this._temp.Add(pre[i][j], +1);
                        }
                        else
                        {
                            this._temp[pre[i][j]]++;
                        }
                    }
                }
            }

            // 新的
            if (next != null)
            {
                for (int i = 0; i < next.Length; i++)
                {
                    if (!this._temp.ContainsKey(next[i]))
                    {
                        this._temp.Add(next[i], -1);
                    }
                    else
                    {
                        this._temp[next[i]]--;
                    }
                }
            }

        }

        private void RemoveAddonActors(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.actors);
            foreach (var item in this._addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.actors);
            }

            this.Collect(pre, config.actors);
            foreach (var item in this._temp)
            {
                if (item.Value < 0) //消失actor
                {
                    if (this._actorEntities.TryGetValue(item.Key, out long entityId))
                    {
                        Parent.Remove(entityId);
                    }
                }
            }
        }

        private void RemoveAddonTriggers(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.triggers);
            foreach (var item in this._addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.triggers);
            }

            this.Collect(pre, config.triggers);
            foreach (var item in this._temp)
            {
                if (item.Value < 0) //消失
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this._activeHandlers.Remove(trigger.localId);
                    }
                }
            }
        }

        private void RemoveAddonZones(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.zones);
            foreach (var item in this._addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.zones);
            }

            this.Collect(pre, config.zones);
            foreach (var item in this._temp)
            {
                if (item.Value < 0) //消失
                {
                    if (this._zoneEntities.TryGetValue(item.Key, out var zone))
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

        public bool TryGetActorEntity(int actorId, out long entityId)
        {
            return this._actorEntities.TryGetValue(actorId, out entityId);
        }

        /// <summary>
        /// 从SceneGroup的管理中移除，不会销毁Entity
        /// </summary>
        /// <param name="actorId"></param>
        public void OnActorRelease(int actorId)
        {
            if (this.TryGetActorEntity(actorId, out var entityId))
            {
                this._actorEntities.Remove(actorId);
            }
        }

        /// <summary>
        /// 创建Actor
        /// </summary>
        /// <param name="actorId"></param>
        public void CreateActor(int actorId)
        {
            if (!this._actorEntities.ContainsKey(actorId) && this.actors.TryGetValue(actorId, out var actor))
            {
                var unit = actor.CreateActor(this);
                this._actorEntities.Add(actor.localId, unit.Id);
            }
        }

        /// <summary>
        /// 获取存活怪物数量
        /// </summary>
        /// <returns></returns>
        public int GetSuiteMonsterCount()
        {
            int res = 0;
            for (int i = 0; i < this.config.actors.Length; i++)
            {
                if (this.config.actors[i] is ConfigSceneGroupActorMonster monster &&
                    this._actorEntities.TryGetValue(monster.localId, out var eid))
                {
                    res++;
                }
            }

            return res;
        }

        #endregion
    }
}