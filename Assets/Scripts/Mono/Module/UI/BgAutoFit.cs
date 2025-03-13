using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TaoTie;

[ExecuteAlways()]
public class BgAutoFit : MonoBehaviour
{
    RectTransform rectTransform;
    Image bg;

    public Sprite bgSprite;

    // Start is called before the first frame update
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        bg = GetComponent<Image>();
    }

    void Start()
    {
        if (bgSprite == null)
            bgSprite = bg.sprite;
        else
            bg.sprite = bgSprite;
        Size();
    }

    void Size()
    {
        //屏幕缩放比
        var screenH = Screen.height;
        var screenW = Screen.width;
        var flagx = (float) Define.DesignScreenWidth / Define.DesignScreenHeight;
        var flagy = (float) screenW / screenH;
        var signFlag = flagx > flagy
            ? (float) Define.DesignScreenWidth / screenW
            : (float) Define.DesignScreenHeight / screenH;
        //图片缩放比
        var width = screenW > screenH ? screenW : screenH;
        var height = screenW < screenH ? screenW : screenH;
        var texture = bgSprite;
        var flag1 = (float) width / texture.bounds.size.x;
        var flag2 = (float) height / texture.bounds.size.y;
        if (flag1 < flag2)
            rectTransform.sizeDelta = new Vector2(flag2 * texture.bounds.size.x * signFlag, height * signFlag);
        else
            rectTransform.sizeDelta = new Vector2(width * signFlag, flag1 * texture.bounds.size.y * signFlag);
    }

    public void SetSprite(Sprite newBgSprite)
    {
        bgSprite = newBgSprite;
        if (bgSprite == null)
            bgSprite = bg.sprite;
        else
            bg.sprite = bgSprite;
        Size();
    }
}