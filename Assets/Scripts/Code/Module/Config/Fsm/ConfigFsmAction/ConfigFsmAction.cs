using System;
using Nino.Serialization;
using UnityEngine;

namespace TaoTie
{
    [NinoSerialize]
    public abstract partial class ConfigFsmAction
    {
        public float NormalizedTime;

        public abstract void Excute(Fsm fsm);
    }
}