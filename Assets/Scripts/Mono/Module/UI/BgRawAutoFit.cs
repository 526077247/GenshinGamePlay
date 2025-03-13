using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TaoTie;

[ExecuteAlways()]
public class BgRawAutoFit : MonoBehaviour
{
    RectTransform rectTransform;
    RawImage bg;

    public Texture bgSprite;

    // Start is called before the first frame update
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        bg = GetComponent<RawImage>();
    }

    void Start()
    {
        if (bgSprite == null)
            bgSprite = bg.texture;
        else
            bg.texture = bgSprite;
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
        var flag1 = (float) width / texture.width;
        var flag2 = (float) height / texture.height;
        if (flag1 < flag2)
            rectTransform.sizeDelta = new Vector2(flag2 * texture.width * signFlag, height * signFlag);
        else
            rectTransform.sizeDelta = new Vector2(width * signFlag, flag1 * texture.height * signFlag);
    }

    public void SetSprite(Texture newBgSprite)
    {
        bgSprite = newBgSprite;
        if (bgSprite == null)
            bgSprite = bg.texture;
        else
            bg.texture = bgSprite;
        Size();
    }
}