using UnityEngine;

namespace TMPro
{
    [RequireComponent(typeof(TMP_Text))]
    [ExecuteAlways]
    public class TextMeshProOutLine : MonoBehaviour
    {
        public float faceDilate;
        public float outlineWidth;
        public Color32 effectColor = Color.black;

        public float underlayOffsetX = 0f;
        public float underlayOffsetY = 0f;
        public float underlayDilate = 0f;

        public void Awake()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            var text = GetComponent<TMP_Text>();
            text.faceDilate = faceDilate;
            text.outlineWidth = outlineWidth;
            text.underlayOffsetX = underlayOffsetX;
            text.underlayOffsetY = underlayOffsetY;
            text.underlayDilate = underlayDilate;

            text.effectColorFloat = new Vector4(effectColor.r / 255f, effectColor.g / 255f, effectColor.b / 255f,
                effectColor.a / 255f);
            
        }

        private void OnValidate()
        {
            Refresh();
        }
    }
}
