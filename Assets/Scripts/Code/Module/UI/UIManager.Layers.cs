
using System.Collections.Generic;
using UnityEngine;
namespace TaoTie
{
    public struct UILayerDefine
    {
        public UILayerNames Name;
        public int PlaneDistance;
        public int OrderInLayer;
    }
    public partial class UIManager
    {
        
        public Camera UICamera { get; private set; }
        public Vector2 Resolution { get; private set; }
        
        private Dictionary<UILayerNames, UILayer> layers;//所有可用的层级
        private GameObject gameObject;

        #region override

        UILayerDefine[] GetConfig()
        {
            UILayerDefine GameBackgroudLayer = new UILayerDefine
            {
                Name = UILayerNames.GameBackgroudLayer,
                PlaneDistance = 1000,
                OrderInLayer = 0,
            };

            //主界面、全屏的一些界面
            UILayerDefine BackgroudLayer = new UILayerDefine
            {
                Name = UILayerNames.BackgroudLayer,
                PlaneDistance = 900,
                OrderInLayer = 1000,
            };

            //游戏内的View层
            UILayerDefine GameLayer = new UILayerDefine
            {
                Name = UILayerNames.GameLayer,
                PlaneDistance = 800,
                OrderInLayer = 1800,
            };
            // 场景UI，如：点击建筑查看建筑信息---一般置于场景之上，界面UI之下
            UILayerDefine SceneLayer = new UILayerDefine
            {
                Name = UILayerNames.SceneLayer,
                PlaneDistance = 700,
                OrderInLayer = 2000,
            };
            //普通UI，一级、二级、三级等窗口---一般由用户点击打开的多级窗口
            UILayerDefine NormalLayer = new UILayerDefine
            {
                Name = UILayerNames.NormalLayer,
                PlaneDistance = 600,
                OrderInLayer = 3000,
            };
            //提示UI，如：错误弹窗，网络连接弹窗等
            UILayerDefine TipLayer = new UILayerDefine
            {
                Name = UILayerNames.TipLayer,
                PlaneDistance = 500,
                OrderInLayer = 4000,
            };
            //顶层UI，如：场景加载
            UILayerDefine TopLayer = new UILayerDefine
            {
                Name = UILayerNames.TopLayer,
                PlaneDistance = 400,
                OrderInLayer = 5000,
            };

            return new UILayerDefine[]
            {
                GameBackgroudLayer ,
                BackgroudLayer,
                GameLayer,
                SceneLayer,
                NormalLayer,
                TipLayer,
                TopLayer,
            };
        }
        
        private void InitLayer()
        {
            Instance = this;
            Log.Info("UILayersComponent Awake");
            var UIRootPath = "Global/UI";
            var EventSystemPath = "EventSystem";
            var UICameraPath = UIRootPath + "/UICamera";
            this.gameObject = GameObject.Find(UIRootPath);
            var eventSystem = GameObject.Find(EventSystemPath);
            var transform = this.gameObject.transform;
            this.UICamera = GameObject.Find(UICameraPath).GetComponent<Camera>();
            GameObject.DontDestroyOnLoad(this.gameObject);
            GameObject.DontDestroyOnLoad(eventSystem);
            this.Resolution = new Vector2(Define.DesignScreen_Width, Define.DesignScreen_Height);//分辨率
            this.layers = new Dictionary<UILayerNames, UILayer>();

            var UILayers = GetConfig();
            for (int i = 0; i < UILayers.Length; i++)
            {
                var layer = UILayers[i];
                var go = new GameObject(layer.Name.ToString())
                {
                    layer = 5
                };
                var trans = go.transform;
                trans.SetParent(transform, false);
                UILayer newLayer = ManagerProvider.RegisterManager<UILayer,UILayerDefine,GameObject>(layer, go,layer.Name.ToString());
                this.layers[layer.Name] = newLayer;
                windowStack[layer.Name] = new LinkedList<string>();
            }
        }

        private void DestroyLayer()
        {
            foreach (var item in this.layers)
            {
                var obj = item.Value.GameObject;
                GameObject.Destroy(obj);
            }
            this.layers.Clear();
            this.layers = null;
            Log.Info("UILayersComponent Dispose");
        }

        #endregion
        
        public void SetCanvasScaleEditorPortrait(bool flag)
        {
            this.layers[UILayerNames.GameLayer].SetCanvasScaleEditorPortrait(flag);
            this.layers[UILayerNames.TipLayer].SetCanvasScaleEditorPortrait(flag);
            this.layers[UILayerNames.TopLayer].SetCanvasScaleEditorPortrait(flag);
            this.layers[UILayerNames.GameBackgroudLayer].SetCanvasScaleEditorPortrait(flag);
        }

    }
}