using UnityEngine;
using UnityEngine.UI;

public class TextColorCtrl : MonoBehaviour 
{

    public Text m_text;
    public TMPro.TMP_Text m_text2;
    public Color m_originTextColor;

    public Outline m_outline;
    public Color m_originOutlineColor;

    public Shadow m_shadow;
    public Color m_originShadowColor;

    public void Awake()
    {
        m_text2 = GetComponent<TMPro.TMP_Text>();
        if (m_text2 != null)
        {
            m_originTextColor = m_text2.color;
        }
        else
        {
            m_text = GetComponent<Text>();
            if(m_text!=null) m_originTextColor = m_text.color;
        }
        
        m_outline = GetComponent<Outline>();
        if(m_outline != null) m_originOutlineColor = m_outline.effectColor;
        
        m_shadow = GetComponent<Shadow>();
        if (m_shadow != null) m_originShadowColor = m_shadow.effectColor;
    }

    public static TextColorCtrl Get(GameObject go)
    {
        var uiTextGrey = go.GetComponent<TextColorCtrl>();
        if (uiTextGrey == null)
        {
            uiTextGrey = go.AddComponent<TextColorCtrl>();
        }

        return uiTextGrey;
    }

    public void SetTextColor(Color color)
    {
        if(m_text!=null) m_text.color = color;
        if(m_text2!=null) m_text2.color = color;
    }

    public void ClearTextColor()
    {
        if(m_text!=null) m_text.color = m_originTextColor;
        if(m_text2!=null) m_text2.color = m_originTextColor;
    }

    public void SetOutlineColor(Color color)
    {
        if(m_outline != null)
            m_outline.effectColor = color;
    }

    public void ClearOutlineColor()
    {
        if (m_outline != null)
            m_outline.effectColor = m_originOutlineColor;
    }

    public void SetShadowColor(Color color)
    {
        if (m_shadow != null)
            m_shadow.effectColor = color;
    }

    public void ClearShadowColor()
    {
        if (m_shadow != null)
            m_shadow.effectColor = m_originShadowColor;
    }
}
