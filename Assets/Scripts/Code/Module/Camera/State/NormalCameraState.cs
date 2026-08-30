using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class NormalCameraState: CameraState
    {
        public override bool IsBlenderState => false;

        public ConfigCamera Config { get; private set; }
        
        private CameraPluginRunner body;
        private CameraPluginRunner head;
        private ListComponent<CameraPluginRunner> others;

        public ICameraEntity follow { get; private set; }
        public ICameraEntity target { get; private set; }

        public static NormalCameraState Create(ConfigCamera config, int priority)
        {
            NormalCameraState res = ObjectPool.Instance.Fetch<NormalCameraState>();
            res.Priority = priority;
            res.Id = IdGenerater.Instance.GenerateId();
            res.Config = config;
            res.Data = CameraStateData.Create();
            res.Data.Fov = res.Config.Fov;
            res.Data.NearClipPlane = res.Config.NearClipPlane;
            res.Data.AvatarFaceDirection = config.AvatarFaceDirection;
            res.IsOver = false;
            res.CreateRunner();
            return res;
        }

        private void CreateRunner()
        {
            if (Config.HeadPlugin != null)
            {
                head = CameraManager.Instance.CreatePluginRunner(Config.HeadPlugin, this);
            }

            if (Config.BodyPlugin != null)
            {
                body =  CameraManager.Instance.CreatePluginRunner(Config.BodyPlugin, this);
            }
            
            if (Config.OtherPlugin != null)
            {
                others = ListComponent<CameraPluginRunner>.Create();
                for (int i = 0; i < Config.OtherPlugin.Length; i++)
                {
                    if(Config.OtherPlugin[i] == null) continue;
                    others.Add(CameraManager.Instance.CreatePluginRunner(Config.OtherPlugin[i], this));
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            CameraManager.Instance.ChangeCursorVisible(Config.VisibleCursor, CursorStateType.Camera);
            CameraManager.Instance.ChangeCursorLock(Config.UnLockCursor, CursorStateType.Camera);
        }
        
        public override void Update()
        {
            if(IsOver) return;
            Calculating();
            body?.Update();
            head?.Update();
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    others[i]?.Update();
                }
            }
        }

        private void Calculating()
        {
            if (target != null)
            {
                Data.TargetForward = target.Forward;
                Data.LookAt = target.Position;
                Data.TargetUp = target.Up;
            }
            else
            {
                Data.TargetForward = Vector3.forward;
                Data.TargetUp = Vector3.up;
            }
            // If no head plugin, keep Orientation in sync with the camera transform
            if (head == null)
            {
                var cam = CameraManager.Instance.MainCamera();
                if (cam != null)
                {
                    Data.Orientation = cam.transform.rotation;
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            
            //this
            target = null;
            follow = null;
            body?.Dispose();
            body = null;
            head?.Dispose();
            head = null;
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    others[i].Dispose();
                }
                others.Dispose();
                others = null;
            }
            
            ObjectPool.Instance.Recycle(this);
        }

        public void SetTarget(ICameraEntity entity)
        {
            this.target = entity;
            this.body?.OnSetTarget();
            this.head?.OnSetTarget();
            if (others != null)
            {
                for (int i = 0; i < this.others.Count; i++)
                {
                    this.others[i]?.OnSetTarget();
                }
            }
        }

        public void SetFollow(ICameraEntity entity)
        {
            this.follow = entity;
            this.body?.OnSetFollow();
            this.head?.OnSetFollow();
            if (others != null)
            {
                for (int i = 0; i < this.others.Count; i++)
                {
                    this.others[i]?.OnSetFollow();
                }
            }
        }

        public void Reset()
        {
            this.body?.Reset();
            this.head?.Reset();
            if (others != null)
            {
                for (int i = 0; i < this.others.Count; i++)
                {
                    this.others[i]?.Reset();
                }
            }
        }
    }
}