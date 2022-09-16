using UnityEngine;
using UnityEngine.UI;
namespace TaoTie
{
    
    //sorting layer名字配置
    public static class SortingLayerNames
    {
        public const string Default = "Default";
        public const string Map = "Map";
        public const string Scene = "Scene";
        public const string Charactor = "Charactor";
        public const string UI = "UI";
    }
    public class UILayer:IManager<UILayerDefine,GameObject>
    {
        public UILayerNames Name;
        public Canvas unity_canvas;
        public CanvasScaler unity_canvas_scaler;
        public GraphicRaycaster unity_graphic_raycaster;
        public RectTransform rectTransform;
        public int top_window_order;
        public int min_window_order;
        protected GameObject _gameObject;
        protected Transform _transform;
        public GameObject gameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
                _transform = _gameObject.transform;
            }
        }
        public Transform transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                _gameObject = _transform.gameObject;
            }
        }

        public void Init(UILayerDefine layer,GameObject gameObject)
        {
            this.Name = layer.Name;
            this.gameObject = gameObject;
            //canvas
            if (!this.gameObject.TryGetComponent(out this.unity_canvas))
            {
                //说明：很坑爹，这里添加UI组件以后transform会Unity被替换掉，必须重新获取
                this.unity_canvas = this.gameObject.AddComponent<Canvas>();
                this.gameObject = this.unity_canvas.gameObject;
            }
            this.unity_canvas.renderMode = RenderMode.ScreenSpaceCamera;
            this.unity_canvas.worldCamera = UILayersManager.Instance.UICamera;
            this.unity_canvas.planeDistance = layer.PlaneDistance;
            this.unity_canvas.sortingLayerName = SortingLayerNames.UI;
            this.unity_canvas.sortingOrder = layer.OrderInLayer;

            //scaler
            if (!this.gameObject.TryGetComponent(out this.unity_canvas_scaler))
            {
                this.unity_canvas_scaler = this.gameObject.AddComponent<CanvasScaler>();
            }
            this.unity_canvas_scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            this.unity_canvas_scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            this.unity_canvas_scaler.referenceResolution = UILayersManager.Instance.Resolution;
            if (Screen.width / Screen.height > Define.DesignScreen_Width / Define.DesignScreen_Height)
                this.unity_canvas_scaler.matchWidthOrHeight = 1;
            else
                this.unity_canvas_scaler.matchWidthOrHeight = 0;

            //raycaster
            if (!this.gameObject.TryGetComponent(out this.unity_graphic_raycaster))
            {
                this.unity_graphic_raycaster = this.gameObject.AddComponent<GraphicRaycaster>();
            }
            // window order
            this.top_window_order = layer.OrderInLayer;
            this.min_window_order = layer.OrderInLayer;
            this.rectTransform = this.gameObject.GetComponent<RectTransform>();
        }
        public void Destroy()
        {
            this.unity_canvas = null;
            this.unity_canvas_scaler = null;
            this.unity_graphic_raycaster = null;
        }
        //设置canvas的worldCamera
        public void SetCanvasWorldCamera(Camera camera)
        {
            var old_camera = this.unity_canvas.worldCamera;
            if (old_camera != camera)
            {
                this.unity_canvas.worldCamera = camera;
            }
        }

        public int GetCanvasLayer()
        {
            return this.transform.gameObject.layer;
        }

        public int PopWindowOder()
        {
            var cur = this.top_window_order;
            this.top_window_order += UIManager.Instance.MaxOderPerWindow;
            return cur;
        }

        public int PushWindowOrder()
        {
            var cur = this.top_window_order;
            this.top_window_order -= UIManager.Instance.MaxOderPerWindow;
            return cur;
        }

        public int GetMinOrderInLayer()
        {
            return this.min_window_order;
        }

        public void SetTopOrderInLayer(int order)
        {
            if (this.top_window_order < order)
            {
                this.top_window_order = order;
            }
        }

        public int GetTopOrderInLayer()
        {
            return this.top_window_order;
        }

        public Vector2 GetCanvasSize()
        {
            return this.rectTransform.rect.size;
        }

        /// <summary>
        /// editor调整canvas scale
        /// </summary>
        /// <param name="flag">是否竖屏</param>
        public void SetCanvasScaleEditorPortrait(bool flag)
        {
            if (flag)
            {
                this.unity_canvas_scaler.referenceResolution = new Vector2(Define.DesignScreen_Height, Define.DesignScreen_Width);
                this.unity_canvas_scaler.matchWidthOrHeight = 0;
            }
            else
            {
                this.unity_canvas_scaler.referenceResolution = UILayersManager.Instance.Resolution;
                this.unity_canvas_scaler.matchWidthOrHeight = 1;
            }
        }
    }
}