﻿using System;
using UnityEngine;

namespace TaoTie
{
    public class ConfigFsmTableLayer
    {
        public string Name = "Base Layer";
        public bool IKPass;
        [HideInInspector]
        public ConfigFsmTableState[] FsmStates = Array.Empty<ConfigFsmTableState>();
        [HideInInspector]
        public ConfigFsmTableItem[,] DataTable = new ConfigFsmTableItem[0, 0];
    }
}