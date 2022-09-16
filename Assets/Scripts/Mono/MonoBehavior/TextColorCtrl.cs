using UnityEngine;
using UnityEngine.UI;

public class TextColorCtrl : MonoBehaviour 
{

    private Text m_text;
    private Color m_originTextColor;

    private Outline m_outline;
    private Color m_originOutlineColor;

    private Shadow m_shadow;
    private Color m_originShadowColor;

    protected Text text
    {
        get
        {
            if (m_text == null)
            {
                m_text = GetComponent<Text>();
                m_originTextColor = m_text.color;
            }

            return m_text;
        }
    }

    protected Outline outline
    {
        get
        {
            if (m_outline == null)
            {
                m_outline = GetComponent<Outline>();

                if(m_outline != null)
                    m_originOutlineColor = m_outline.effectColor;
            }

            return m_outline;
        }
    }

    protected Shadow shadow
    {
        get
        {
            if (m_shadow == null)
            {
                m_shadow = GetComponent<Shadow>();

                if (m_shadow != null)
                    m_originShadowColor = m_shadow.effectColor;
            }

            return m_shadow;
        }
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
        text.color = color;
    }

    public void ClearTextColor()
    {
        text.color = m_originTextColor;
    }

    public void SetOutlineColor(Color color)
    {
        var _outline = outline;
        if(_outline != null)
            _outline.effectColor = color;
    }

    public void ClearOutlineColor()
    {
        var _outline = outline;
        if (_outline != null)
            _outline.effectColor = m_originOutlineColor;
    }

    public void SetShadowColor(Color color)
    {
        var _shadow = shadow;
        if (_shadow != null)
            _shadow.effectColor = color;
    }

    public void ClearShadowColor()
    {
        var _shadow = shadow;
        if (_shadow != null)
            _shadow.effectColor = m_originShadowColor;
    }
}
