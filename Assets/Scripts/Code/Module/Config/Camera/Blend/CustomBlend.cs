using System.Collections.Generic;

using UnityEngine;

namespace TaoTie
{
    public class CustomBlend
    {
        [SerializeField] private int _from;
        [SerializeField] private int _to;
        [SerializeField] private BlendDefinition _definition;

        public int from => _from;
        public int to => _to;
        public BlendDefinition definition => _definition;
    }
}