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
        public const string Character = "Character";
        public const string UI = "UI";
    }
    public class UILayer:IManager<UILayerDefine,GameObject>
    {
        public UILayerNames Name { get; private set; }
        public Canvas Canvas{ get; private set; }
        public CanvasScaler CanvasScaler{ get; private set; }
        public GraphicRaycaster GraphicRaycaster{ get; private set; }
        public RectTransform RectTransform{ get; private set; }
        public GameObject GameObject{ get; private set; }

        public void Init(UILayerDefine layer,GameObject gameObject)
        {
            this.Name = layer.Name;
            this.GameObject = gameObject;

            //canvas
            if (!this.GameObject.TryGetComponent(out Canvas canvas))
            {
                //说明：很坑爹，这里添加UI组件以后transform会Unity被替换掉，必须重新获取
                this.Canvas = this.GameObject.AddComponent<Canvas>();
                this.GameObject = this.Canvas.gameObject;
            }
            else
            {
                this.Canvas = canvas;
            }
            this.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            this.Canvas.worldCamera = UIManager.Instance.UICamera;
            this.Canvas.planeDistance = layer.PlaneDistance;
            this.Canvas.sortingLayerName = SortingLayerNames.UI;
            this.Canvas.sortingOrder = layer.OrderInLayer;

            //scaler
            if (!this.GameObject.TryGetComponent(out CanvasScaler canvasScaler))
            {
                this.CanvasScaler = this.GameObject.AddComponent<CanvasScaler>();
            }
            else
            {
                this.CanvasScaler = canvasScaler;
            }
            this.CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            this.CanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            this.CanvasScaler.referenceResolution = UIManager.Instance.Resolution;
            if (Screen.width / Screen.height > Define.DesignScreen_Width / Define.DesignScreen_Height)
                this.CanvasScaler.matchWidthOrHeight = 1;
            else
                this.CanvasScaler.matchWidthOrHeight = 0;

            //raycaster
            if (!this.GameObject.TryGetComponent(out GraphicRaycaster graphicRaycaster))
            {
                this.GraphicRaycaster = this.GameObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                this.GraphicRaycaster = graphicRaycaster;
            }

            this.RectTransform = this.GameObject.GetComponent<RectTransform>();
        }
        public void Destroy()
        {
            this.GameObject = null;
            this.Canvas = null;
            this.CanvasScaler = null;
            this.GraphicRaycaster = null;
            this.RectTransform = null;
        }

        /// <summary>
        /// editor调整canvas scale
        /// </summary>
        /// <param name="flag">是否竖屏</param>
        public void SetCanvasScaleEditorPortrait(bool flag)
        {
            if (flag)
            {
                this.CanvasScaler.referenceResolution = new Vector2(Define.DesignScreen_Height, Define.DesignScreen_Width);
                this.CanvasScaler.matchWidthOrHeight = 0;
            }
            else
            {
                this.CanvasScaler.referenceResolution = UIManager.Instance.Resolution;
                this.CanvasScaler.matchWidthOrHeight = 1;
            }
        }
    }
}