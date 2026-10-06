using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public class CancelRaycastTarget
{
    static CancelRaycastTarget()
    {
        ObjectFactory.componentWasAdded += ComponentWasAdded;
    }

    private static void ComponentWasAdded(Component comp)
    {
        if (comp.GetType() == typeof(Text))
        {
            Text tmp = (Text) comp;
            tmp.raycastTarget = false;
            // 如果禁止使用Text
            // EditorApplication.CallbackFunction Update = null;
            // Update = () =>
            // {
            //     var obj = comp.gameObject;
            //     GameObject.DestroyImmediate(comp);
            //     obj.AddComponent<TextMeshProUGUI>();
            //     EditorApplication.update -= Update;
            // };
            // EditorApplication.update += Update;
        }
        else if (comp.GetType() == typeof(TextMeshProUGUI))
        {
            TextMeshProUGUI tmp = (TextMeshProUGUI) comp;
            tmp.raycastTarget = false;
        }
        else if (comp.GetType() == typeof(Image))
        {
            Image image = (Image) comp;
            image.raycastTarget = false;
        }
        else if (comp.GetType() == typeof(RawImage))
        {
            RawImage image = (RawImage) comp;
            image.raycastTarget = false;
        }
    }
}
