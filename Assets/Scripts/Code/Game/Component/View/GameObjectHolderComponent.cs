using System;
using UnityEngine;
using UnityEngine.Events;

namespace TaoTie
{
    public partial class GameObjectHolderComponent:Component,IComponent
    {
        [Timer(TimerType.DestroyEffect)]
        public class DestroyEffectTimer: ATimer<EffectInfo>
        {
            public override void Run(EffectInfo self)
            {
                try
                {
                    self.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.ConfigId}\n{e}");
                }
            }
        }
        public class EffectInfo:IDisposable
        {
            public static EffectInfo Create()
            {
                var res = ObjectPool.Instance.Fetch<EffectInfo>();
                res.IsDispose = false;
                return res;
            }

            private bool IsDispose;
            public GameObject obj;
            public int ConfigId;
            public GameObjectHolderComponent Parent;
            public long TimerId;
            public void Dispose()
            {
                if (IsDispose)
                {
                    return;
                }

                IsDispose = true;
                GameTimerManager.Instance.Remove(ref TimerId);
                Parent.RemoveEffect(ConfigId);
                Parent = default;
                ConfigId = default;
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                obj = null;
                ObjectPool.Instance.Recycle(this);
            }
        }
        public Transform EntityView;

        public Transform TriggerRoot;

        public ReferenceCollector Collector;

        DictionaryComponent<long, Collider> Triggers;

        DictionaryComponent<int, EffectInfo> Effects;
        #region override

        public void Init()
        {
            Triggers = DictionaryComponent<long, Collider>.Create();
            Effects = DictionaryComponent<int, EffectInfo>.Create();
            LoadGameObjectAsync().Coroutine();
        }

        public async ETTask LoadGameObjectAsync()
        {
            var unit = this.GetParent<Unit>();
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(unit.Config.Perfab);
            if (this.IsDispose)
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }

            Animator = obj.GetComponentInChildren<Animator>();
            EntityView = obj.transform;
            Collector = obj.GetComponent<ReferenceCollector>();
            EntityView.SetParent(this.Parent.Parent.GameObjectRoot);
            var ec = obj.GetComponent<EntityComponent>();
            if (ec == null) ec = obj.AddComponent<EntityComponent>();
            ec.Id = this.Id;

            ec.EntityType = unit.Type;
            GameObject tRoot = new GameObject("TriggerRoot");
            TriggerRoot = tRoot.transform;
            TriggerRoot.SetParent(EntityView);
            TriggerRoot.localPosition = Vector3.zero;
            EntityView.position = unit.Position;
            EntityView.rotation = unit.Rotation;
            Messager.Instance.AddListener<Unit,Vector3>(Id,MessageId.ChangePositionEvt,OnChanePosition);
            Messager.Instance.AddListener<Unit,Quaternion>(Id,MessageId.ChangeRotationEvt,OnChaneRotation);
            Messager.Instance.AddListener<Unit>(Id,MessageId.MoveStart,MoveStart);
            Messager.Instance.AddListener<Unit>(Id,MessageId.MoveStop,MoveStop);
            // var hud = unit.GetComponent<HudComponent>();
            // if (hud != null)
            // {
            //     HudSystem hudSys = ManagerProvider.GetManager<HudSystem>();
            //     hudSys?.ShowHeadInfo(hud.Info);
            // }
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<Unit,Vector3>(Id,MessageId.ChangePositionEvt,OnChanePosition);
            Messager.Instance.RemoveListener<Unit,Quaternion>(Id,MessageId.ChangeRotationEvt,OnChaneRotation);
            Messager.Instance.RemoveListener<Unit>(Id,MessageId.MoveStart,MoveStart);
            Messager.Instance.RemoveListener<Unit>(Id,MessageId.MoveStop,MoveStop);
            foreach (var v in Triggers)
            {
                var value = v.Value;
                GameObject.DestroyImmediate(value.gameObject);
            }
            Triggers.Clear();
            Triggers = null;
            if(TriggerRoot!=null)
                GameObject.Destroy(TriggerRoot.gameObject);
            foreach (var v in Effects)
            {
                var value = v.Value;
                value.Dispose();
            }
            Effects.Clear();
            Effects = null;
            if(EntityView!=null)
                GameObjectPoolManager.Instance.RecycleGameObject(EntityView.gameObject);
        }

        #endregion

        public void OnChanePosition(Unit unit,Vector3 old)
        {
            EntityView.position = unit.Position;
        }

        public void OnChaneRotation(Unit unit, Quaternion old)
        {
            EntityView.rotation = unit.Rotation;
        }

        public async ETTask AddEffect(int id)
        {
            while (EntityView==null)
            {
                await TimerManager.Instance.WaitAsync(1);
                if(this.IsDispose) return;
            }
            
            // var effectConfig = EffectConfigCategory.Instance.Get(id);
            // Transform root = null;
            // if(Collector!=null)
            //     root = Collector.Get<GameObject>(effectConfig.MountPoint)?.transform;
            // if(root==null) return;
            //
            // var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(effectConfig.Prefab);
            // if (this.IsDispose)
            // {
            //     GameObjectPoolManager.Instance?.RecycleGameObject(obj);
            //     return;
            // }
            // obj.transform.SetParent(root);
            // obj.transform.localPosition = new Vector3(effectConfig.RelativePos[0],effectConfig.RelativePos[1],effectConfig.RelativePos[2]);
            // obj.transform.localEulerAngles = new Vector3(effectConfig.RelativeRotation[0],effectConfig.RelativeRotation[1],effectConfig.RelativeRotation[2]);
            // obj.transform.localScale = Vector3.one;
            // if (effectConfig.IsMount == 0)
            // {
            //     obj.transform.SetParent(null);
            // }
            //
            // EffectInfo info = EffectInfo.Create();
            // info.obj = obj;
            // info.ConfigId = id;
            // info.Parent = this;
            // if (effectConfig.PlayTime >= 0)
            // {
            //     info.TimerId = GameTimerManager.Instance.NewOnceTimer(GameTimerManager.Instance.GetTimeNow() + effectConfig.PlayTime, 
            //         TimerType.DestroyEffect, info);
            // }
        }

        public void RemoveEffect(int id)
        {
            if (Effects.TryGetValue(id, out var info))
            {
                info.Dispose();
                Effects.Remove(id);
            }
        }
        
        public async ETTask AddOBBTrigger(long id,Vector3 size,TriggerType type,EntityType entityType,UnityAction<long,TriggerType,Vector3> onCollider)
        {
            while (TriggerRoot==null)
            {
                await TimerManager.Instance.WaitAsync(1);
                if(IsDispose) return;
            }
            var obj = new GameObject(id.ToString());
            var tc = obj.AddComponent<TriggerComponent>();
            tc.CastEntityType = entityType;
            if (type == TriggerType.Enter || type == TriggerType.All)
            {
                tc.OnTriggerEnterEvt += onCollider;
            }
            if (type == TriggerType.Exit || type == TriggerType.All)
            {
                tc.OnTriggerExitEvt += onCollider;
            }
            var collider = obj.AddComponent<BoxCollider>();
            collider.size = size;
            collider.isTrigger = true;
            obj.transform.SetParent(TriggerRoot);
            obj.transform.localPosition = Vector3.zero;
            Triggers.Add(id,collider);
        }
        
        public async ETTask AddSphereTrigger(long id,float radius,TriggerType type,EntityType entityType,UnityAction<long,TriggerType,Vector3> onCollider)
        {
            while (TriggerRoot==null)
            {
                await TimerManager.Instance.WaitAsync(1);
                if(IsDispose) return;
            }
            var obj = new GameObject(id.ToString());
            var tc = obj.AddComponent<TriggerComponent>();
            tc.CastEntityType = entityType;
            if (type == TriggerType.Enter || type == TriggerType.All)
            {
                tc.OnTriggerEnterEvt += onCollider;
            }
            if (type == TriggerType.Exit || type == TriggerType.All)
            {
                tc.OnTriggerExitEvt += onCollider;
            }
            var collider = obj.AddComponent<SphereCollider>();
            collider.radius = radius;
            obj.transform.SetParent(TriggerRoot);
            obj.transform.localPosition = Vector3.zero;
            Triggers.Add(id,collider);
        }
        
        public void RemoveTrigger(long id)
        {
            if(Triggers==null) return;
            if (Triggers.TryGetValue(id,out var trigger))
            {
                GameObject.DestroyImmediate(trigger);
                Triggers.Remove(id);
            }
        }
    }
}