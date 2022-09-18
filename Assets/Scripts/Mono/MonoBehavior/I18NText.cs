using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    public class I18NText : MonoBehaviour
    {

        public string key;
        private Text m_Text;
        private TMPro.TMP_Text m_MeshText;
        void Awake()
        {
            m_Text = GetComponent<Text>();
            m_MeshText = GetComponent<TMPro.TMP_Text>();
        }

        private void OnEnable()
        {
            OnSwitchLanguage();
            I18NBridge.Instance.OnLanguageChangeEvt += OnSwitchLanguage;
        }

        private void OnDisable()
        {
            I18NBridge.Instance.OnLanguageChangeEvt -= OnSwitchLanguage;
        }

        private void OnSwitchLanguage()
        {
            if (m_Text != null)
                m_Text.text = I18NBridge.Instance.GetText(key);
            if (m_MeshText != null)
                m_MeshText.text = I18NBridge.Instance.GetText(key);
        }
    }


}