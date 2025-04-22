using UnityEngine;

namespace TaoTie
{
    public partial class EnvironmentManager
    {
        private int TexId = Shader.PropertyToID("_Tex");
        private int Tex2Id = Shader.PropertyToID("_Tex2");
        private int TintId = Shader.PropertyToID("_Tint");
        private int Tint2Id = Shader.PropertyToID("_Tint2");
        private int BlendCubemapsId = Shader.PropertyToID("_BlendCubemaps");
        
        private partial void ApplySkybox(EnvironmentInfo info)
        {
            //todo:
            RenderSettings.skybox = skybox;
            if (skybox != null)
            {
                if (info.IsBlender)
                {
                    skybox.SetTexture(Tex2Id, info.SkyCube2);
                    skybox.SetColor(Tint2Id, info.TintColor2);
                    skybox.SetFloat(BlendCubemapsId, info.Progress);
                }
                else
                {
                    skybox.SetFloat(BlendCubemapsId, 0);
                }

                skybox.SetColor(TintId, info.TintColor);
                skybox.SetTexture(TexId, info.SkyCube);
            }
        }
    }
}