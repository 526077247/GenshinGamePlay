using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaoTie
{
    public class ConfigCameras
    {
        [SerializeField] private ConfigFreeLookCamera _defaultCamera;

        [SerializeField] private ConfigCamera[] _cameras;

        [NotNull] [SerializeField] private BlendDefinition _defaultBlend = new BlendDefinition();

        [TableList] [SerializeField] private CustomBlend[] _customSetting;

        public ConfigCamera[] cameras => _cameras;
        public ConfigFreeLookCamera defaultCamera => _defaultCamera;
        public BlendDefinition defaultBlend => _defaultBlend;
        public CustomBlend[] customSetting => _customSetting;
    }
}