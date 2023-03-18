using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    /// <summary>
    /// hud
    /// </summary>
    public sealed class FightText : IDisposable
    {
        private UIHudView _uiHudView;
        public RectTransform rect { get; private set; }
        public AttackResult attackResult { get; private set; } = new AttackResult();
        public long expire_time { get; private set; }
        private bool _isDisposable = true;

        public async ETTask OnInit(UIHudView uiHudView)
        {
            _isDisposable = false;
            _uiHudView = uiHudView;
            string res_path = "UIGame/UIBattle/Prefabs/UIFightText.prefab";
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(res_path);
            if (_isDisposable)//等加载回来可能已经销毁了
            {
                GameObjectPoolManager.Instance.RecycleGameObject(obj);
                return;
            }
            rect = obj.GetComponent<RectTransform>();
            OnGameObjectLoad();
        }

        public void OnGameObjectLoad()
        {
            if (_uiHudView != null && rect != null)
            {
                rect.SetParent(_uiHudView.GetTransform());
                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
            }
            UpdateText();
        }

        /// <summary>
        /// 回收
        /// </summary>
        public  void Dispose()
        {
            if (_isDisposable) return;
            _isDisposable = true;
            if (rect != null)
            {
                GameObjectPoolManager.Instance?.RecycleGameObject(rect.gameObject);
                rect = null;
            }
            _uiHudView = null;
            ObjectPool.Instance.Recycle(this);
        }

        public void SetData(AttackResult _attackResult, long _expire_time)
        {
            attackResult = _attackResult;
            expire_time = _expire_time;
            Debug.Log("zzz attackresult" + attackResult.FinalRealDamage);
            UpdateText();
         }

        public void UpdateText()
        {
            if (rect == null)
            {
                return;
            }
            var text = rect.GetComponentInChildren<Text>();
            if (text != null)
            {
                Debug.Log("zzz fuzhi " + attackResult.FinalRealDamage);
                text.text = attackResult.FinalRealDamage.ToString();
            }
        }

        public void SetActive(bool value)
        {
            if (rect != null)
            {
                rect.gameObject.SetActive(value);
            }
        }
    }
}