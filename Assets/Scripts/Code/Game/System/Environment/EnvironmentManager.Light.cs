using UnityEngine;

namespace TaoTie
{
    public partial class EnvironmentManager
    {
        private Light dirLight;
        private partial void ApplyLight(EnvironmentInfo info)
        {
            if (dirLight == null) return;
            dirLight.enabled = info.UseDirLight;
            if(!info.UseDirLight) return;
            dirLight.color = info.LightColor;
            dirLight.transform.eulerAngles = info.LightDir;
            dirLight.intensity = info.LightIntensity;
            dirLight.shadows = info.LightShadows;
        }
    }
}