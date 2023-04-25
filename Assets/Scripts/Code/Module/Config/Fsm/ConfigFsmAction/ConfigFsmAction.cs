using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public abstract partial class ConfigFsmAction
    {
        public float normalizedTime;

        public abstract void Excute(Fsm fsm);
    }
}