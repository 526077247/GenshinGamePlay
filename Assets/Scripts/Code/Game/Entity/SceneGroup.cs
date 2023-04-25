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
            configId = p1.Id;
            variable = DynDictionary.Create();
            _actorEntities = new Dictionary<int, long>();
            _zoneEntities = new Dictionary<int, long>();
            _timerTrigger = new Dictionary<int, long>();
            _activeHandlers = new LinkedList<int>();
            AfterLoadFromDB();
            if (config.RandSuite && config.Suites != null && config.Suites.Length > 0)
            {
                this._temp.Clear();
                int total = 0;
                for (int i = 0; i < config.Suites.Length; i++)
                {
                    this._temp.Add(config.Suites[i].LocalId, config.Suites[i].RandWeight);
                    total += config.Suites[i].RandWeight;
                }

                if (total == 0)
                {
                    Log.Error("随机group失败 totalWeight == 0! sceneGroupId=" + config.Id);
                    this.ChangeSuite(config.Suites[0].LocalId);
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

                Log.Error("随机suite失败 sceneGroupId=" + config.Id);
                this.ChangeSuite(config.Suites[0].LocalId);
            }
            else
            {
                this.ChangeSuite(config.InitSuite);
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
        public DynDictionary variable { get; set; }
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

            this.actors = new Dictionary<int, ConfigSceneGroupActor>(this.config.Actors.Length);
            this.routes = new Dictionary<int, ConfigRoute>(this.config.Route != null ? this.config.Route.Length : 0);
            this.triggers = new Dictionary<int, ConfigSceneGroupTrigger>(this.config.Triggers.Length);
            this.zones = new Dictionary<int, ConfigSceneGroupZone>(this.config.Zones.Length);
            this.suite = new Dictionary<int, ConfigSceneGroupSuites>(this.config.Suites.Length);

            for (int i = 0; i < (this.config.Actors == null ? 0 : this.config.Actors.Length); i++)
            {
                this.actors.Add(this.config.Actors[i].LocalId, this.config.Actors[i]);
            }

            for (int i = 0; i < (this.config.Route == null ? 0 : this.config.Route.Length); i++)
            {
                this.routes.Add(this.config.Route[i].LocalId, this.config.Route[i]);
            }

            for (int i = 0; i < (this.config.Triggers == null ? 0 : this.config.Triggers.Length); i++)
            {
                this.triggers.Add(this.config.Triggers[i].LocalId, this.config.Triggers[i]);
            }

            for (int i = 0; i < (this.config.Zones == null ? 0 : this.config.Zones.Length); i++)
            {
                this.zones.Add(this.config.Zones[i].LocalId, this.config.Zones[i]);
            }

            for (int i = 0; i < (this.config.Suites == null ? 0 : this.config.Suites.Length); i++)
            {
                this.suite.Add(this.config.Suites[i].LocalId, this.config.Suites[i]);
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
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.Actors : null, config.Actors);
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
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.Triggers : null,
                config.Triggers);

            foreach (var item in this._temp)
            {
                if (item.Value > 0) //新增
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this._activeHandlers.AddLast(trigger.LocalId);
                        if (trigger is ConfigGameTimeChangeTrigger timeEventTrigger)
                        {
                            var timerId = GameTimerManager.Instance.NewOnceTimer(timeEventTrigger.GameTime,
                                TimerType.GameTimeEventTrigger, this);
                            this._timerTrigger.Add(trigger.LocalId, timerId);
                        }
                    }
                }
                else if (item.Value < 0) //消失
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this._activeHandlers.Remove(trigger.LocalId);
                        if (trigger is ConfigGameTimeChangeTrigger &&
                            this._timerTrigger.TryGetValue(trigger.LocalId, out var timerId))
                        {
                            GameTimerManager.Instance.Remove(ref timerId);
                            this._timerTrigger.Remove(trigger.LocalId);
                        }
                    }
                }
            }
        }

        private void ChangeZones(ConfigSceneGroupSuites config)
        {
            this.Collect(this.CurGroupSuitesConfig != null ? this.CurGroupSuitesConfig.Zones : null, config.Zones);

            foreach (var item in this._temp)
            {
                if (item.Value > 0) //新增
                {
                    if (!this._zoneEntities.ContainsKey(item.Key) && this.zones.TryGetValue(item.Key, out var zone))
                    {
                        var unit = zone.CreateZone(this);
                        this._zoneEntities[zone.LocalId] = unit.Id;
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
                                               !this._addOnSuiteConfig.Contains(config.LocalId)))
            {
                if (this._addOnSuiteConfig == null)
                {
                    this._addOnSuiteConfig = new HashSet<int>();
                }

                this.AddonTriggers(config);
                this.AddonActors(config);
                this.AddonZones(config);
                this._addOnSuiteConfig.Add(config.LocalId);
                Messager.Instance.Broadcast(Id, MessageId.SceneGroupEvent, new SuiteLoadEvent()
                {
                    SuiteId = curSuiteId,
                    IsAddOn = true,
                });
            }
        }

        private void AddonActors(ConfigSceneGroupSuites config)
        {
            for (int i = 0; i < config.Actors.Length; i++)
            {
                if (!this._actorEntities.ContainsKey(config.Actors[i]) &&
                    this.actors.TryGetValue(config.Actors[i], out var actor))
                {
                    var unit = actor.CreateActor(this);
                    this._actorEntities.Add(actor.LocalId, unit.Id);
                }
            }
        }

        private void AddonTriggers(ConfigSceneGroupSuites config)
        {
            for (int i = 0; i < config.Triggers.Length; i++)
            {
                if (this.triggers.TryGetValue(config.Triggers[i], out var trigger) &&
                    !this._activeHandlers.Contains(trigger.LocalId))
                {
                    this._activeHandlers.AddLast(trigger.LocalId);
                }
            }
        }

        private void AddonZones(ConfigSceneGroupSuites config)
        {
            for (int i = 0; i < config.Zones.Length; i++)
            {
                if (!this._zoneEntities.ContainsKey(config.Zones[i]) &&
                    this.zones.TryGetValue(config.Zones[i], out var zone))
                {
                    var unit = zone.CreateZone(this);
                    this._zoneEntities[zone.LocalId] = unit.Id;
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
                this._addOnSuiteConfig.Contains(config.LocalId))
            {
                this.RemoveAddonZones(config);
                this.RemoveAddonActors(config);
                this.RemoveAddonTriggers(config);
                this._addOnSuiteConfig.Remove(config.LocalId);
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
                pre.Add(this.CurGroupSuitesConfig.Actors);
            foreach (var item in this._addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.Actors);
            }

            this.Collect(pre, config.Actors);
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
                pre.Add(this.CurGroupSuitesConfig.Triggers);
            foreach (var item in this._addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.Triggers);
            }

            this.Collect(pre, config.Triggers);
            foreach (var item in this._temp)
            {
                if (item.Value < 0) //消失
                {
                    if (this.triggers.TryGetValue(item.Key, out var trigger))
                    {
                        this._activeHandlers.Remove(trigger.LocalId);
                    }
                }
            }
        }

        private void RemoveAddonZones(ConfigSceneGroupSuites config)
        {
            List<int[]> pre = new List<int[]>();
            if (this.CurGroupSuitesConfig != null)
                pre.Add(this.CurGroupSuitesConfig.Zones);
            foreach (var item in this._addOnSuiteConfig)
            {
                var conf = this.suite[item];
                pre.Add(conf.Zones);
            }

            this.Collect(pre, config.Zones);
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
                this._actorEntities.Add(actor.LocalId, unit.Id);
            }
        }

        /// <summary>
        /// 获取存活怪物数量
        /// </summary>
        /// <returns></returns>
        public int GetSuiteMonsterCount()
        {
            int res = 0;
            for (int i = 0; i < this.config.Actors.Length; i++)
            {
                if (this.config.Actors[i] is ConfigSceneGroupActorMonster monster &&
                    this._actorEntities.TryGetValue(monster.LocalId, out var eid))
                {
                    res++;
                }
            }

            return res;
        }

        #endregion
    }
}