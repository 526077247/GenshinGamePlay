using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class ConfigCameraRoute 
    {
        [NotNull] [SerializeField] private ConfigCameraRoutePoint[] _points;
        [SerializeField] private int _resolution;
        [SerializeField] private bool _loop;
        public ConfigCameraRoutePoint[] points => _points;
        public int resolution => _resolution;
        public bool loop => _loop;
    }
}