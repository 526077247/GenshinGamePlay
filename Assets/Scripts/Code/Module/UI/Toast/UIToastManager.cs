using UnityEngine;
namespace TaoTie
{
    public class UIToastManager: IManager
    {
        private const string toastPrefab = "UI/UICommon/Prefabs/UIToast.prefab";
        public static UIToastManager Instance;
        
        public void Init()
        {
            Instance = this;
            GameObjectPoolManager.GetInstance().PreLoadGameObjectAsync(toastPrefab, 3).Coroutine();
        }

        public void Destroy()
        {
            Instance = null;
        }

        public async ETTask ShowToast(string content, int time = 1500)
        {
            var obj = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(toastPrefab);
            obj.transform.SetParent(UIManager.Instance.GetLayer(UILayerNames.TipLayer).RectTransform,false);
            var canvas = obj.GetComponent<CanvasGroup>();
            canvas.alpha = 1;
            var txt = obj.GetComponentInChildren<TMPro.TMP_Text>();
            txt.SetText(content);
            await TimerManager.Instance.WaitAsync(time);
            var startTime = TimerManager.Instance.GetTimeNow();
            while (true)
            {
                await TimerManager.Instance.WaitAsync(1);
                var timeNow = TimerManager.Instance.GetTimeNow();
                if (timeNow > startTime + 500)
                {
                    canvas.alpha = 0;
                    break;
                }
                canvas.alpha = Mathf.Lerp(1, 0, (timeNow - startTime) / 500f);
            }
            GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
        }
    }
}