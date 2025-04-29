using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    /// <summary>
    /// 伤害飘字
    /// </summary>
    public sealed class DamageText : IDisposable
    {
        public static DamageText Create(UIDamageView uiDamageView)
        {
            var res = ObjectPool.Instance.Fetch<DamageText>();
            res.OnInit(uiDamageView).Coroutine();
            return res;
        }

        public bool IsDispose { get; private set; }
        public long ExpireTime { get; private set; }

        private UIDamageView uiDamageView;
        private RectTransform rect;
        private int showDmg;
        private Vector3 showPos;

        async ETTask OnInit(UIDamageView uiDamageView)
        {
            IsDispose = false;
            this.uiDamageView = uiDamageView;
            string resPath = "UIGame/UIMain/Prefabs/UIFightText.prefab";
            var obj = await GameObjectPoolManager.GetInstance().GetGameObjectAsync(resPath);
            if (IsDispose) //加载过来已经被销毁了
            {
                GameObjectPoolManager.GetInstance().RecycleGameObject(obj);
                return;
            }

            rect = obj.GetComponent<RectTransform>();
            OnGameObjectLoad();
        }

        void OnGameObjectLoad()
        {
            if (uiDamageView != null && rect != null)
            {
                rect.SetParent(uiDamageView.GetTransform());
                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
            }

            UpdateText();
        }

        public void SetData(int finalDmg, Vector3 pos, long time)
        {
            showDmg = finalDmg;
            showPos = pos;
            ExpireTime = time;
            UpdateText();
        }

        public void UpdateText()
        {
            if (rect == null)
            {
                return;
            }

            var text = rect.GetComponentInChildren<TMPro.TMP_Text>();
            if (text != null)
            {
                text.text = showDmg.ToString();
                var mainCamera = CameraManager.Instance.MainCamera();
                if (mainCamera != null)
                {
                    Vector2 pt = UIManager.Instance.ScreenPointToUILocalPoint(uiDamageView.GetRectTransform(),
                        mainCamera.WorldToScreenPoint(showPos));
                    rect.anchoredPosition = pt;
                }
            }
        }

        public void Dispose()
        {
            if (IsDispose) return;
            IsDispose = true;
            if (rect != null)
            {
                GameObjectPoolManager.GetInstance().RecycleGameObject(rect.gameObject);
                rect = null;
            }

            ObjectPool.Instance.Recycle(this);
        }
    }
}