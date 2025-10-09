using UnityEngine;

namespace TaoTie
{
    public class UIToast : UIBaseView, IOnCreate,IOnEnable<string>,IOnEnable<I18NKey>,
        IOnEnable<string, int>, IOnEnable<I18NKey, int>
    {
        public const string PrefabPath = "UI/UICommon/Prefabs/UIToast.prefab";

        public UIMonoBehaviour<CanvasGroup> CanvasGroup;
        public UITextmesh Text;
        public void OnCreate()
        {
            GameObjectPoolManager.GetInstance().AddPersistentPrefabPath(PrefabPath);
            CanvasGroup = AddComponent<UIMonoBehaviour<CanvasGroup>>();
            Text = AddComponent<UITextmesh>("Text");
        }
        public void OnEnable(string content)
        {
            Text.SetText(content);
            OnEnableAsync().Coroutine();
        }

        public void OnEnable(I18NKey key)
        {
            Text.SetI18NKey(key);
            OnEnableAsync().Coroutine();
        }

        public void OnEnable(string content, int time)
        {
            Text.SetText(content);
            OnEnableAsync(time).Coroutine();
        }

        public void OnEnable(I18NKey key, int time)
        {
            Text.SetI18NKey(key);
            OnEnableAsync(time).Coroutine();
        }

        private async ETTask OnEnableAsync(int time = 1500)
        {
            var canvas = CanvasGroup.GetComponent();
            canvas.alpha = 1;
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
            await CloseSelf();
        }
    }
}