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

        public CameraPluginRunner Body => body;

        public static NormalCameraState Create(ConfigCamera config, int priority)
        {
            NormalCameraState res = ObjectPool.Instance.Fetch<NormalCameraState>();
            res.Priority = priority;
            res.Id = IdGenerater.Instance.GenerateId();
            res.Config = config;
            res.Data = CameraStateData.Create();
            res.Data.Fov = res.Config.Fov;
            res.Data.Orthographicn = res.Config.Orthographicn;
            res.Data.OrthographicnSize = res.Config.OrthographicnSize;
            res.Data.NearClipPlane = res.Config.NearClipPlane;
            res.Data.FarClipPlane = res.Config.FarClipPlane;
            res.Data.AvatarFaceDirection = config.AvatarFaceDirection;
            res.Data.Position = Vector3.zero;
            res.Data.LookAt = Vector3.zero;
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
            else if (follow != null)
            {
                // 没有 target 时用 follow 兜底：Body 插件的机位本来就是围绕 follow 算出来的，
                // LookAt 必须跟着它走。这里曾经是空缺，于是过渡期的焦点距离会落到 follow 身上，
                Data.TargetForward = follow.Forward;
                Data.LookAt = follow.Position;
                Data.TargetUp = follow.Up;
            }
            else
            {
                // 既无 target 也无 follow：用当前相机自身前方一点，保证 LookAt 始终有效，
                // 不让过渡期的焦点距离退化成 0。
                var cam = CameraManager.Instance.MainCamera();
                var pos = cam != null ? cam.transform.position : Vector3.zero;
                var rot = cam != null ? cam.transform.rotation : Quaternion.identity;
                Data.TargetForward = rot * Vector3.forward;
                Data.LookAt = pos + Data.TargetForward;
                Data.TargetUp = rot * Vector3.up;
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
