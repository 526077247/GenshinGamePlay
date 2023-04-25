using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    /// <summary>
    /// hud
    /// </summary>
    public sealed class FightText
    {
        private UIHudView _uiHudView;
        public RectTransform rect { get; private set; }
        int show_dmg;
        Vector3 show_pos;
        public long expire_time { get; private set; }

        public async ETTask OnInit(UIHudView uiHudView)
        {
            _uiHudView = uiHudView;
            string res_path = "UIGame/UIBattle/Prefabs/UIFightText.prefab";
            var obj = await GameObjectPoolManager.Instance.GetGameObjectAsync(res_path);
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

        public void SetData(int final_dmg, Vector3 pos , long time)
        {
            show_dmg = final_dmg;
            show_pos = pos;
            expire_time = time;
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
                text.text = show_dmg.ToString();
                Vector2 pt = Camera.main.WorldToScreenPoint(show_pos) * UIManager.Instance.ScreenSizeflag;
                rect.anchoredPosition = pt;
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