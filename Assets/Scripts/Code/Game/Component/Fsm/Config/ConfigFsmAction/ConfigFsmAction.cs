using System;
using UnityEngine;

namespace TaoTie
{

    public abstract class ConfigFsmAction
    {
        public float normalizedTime;

        public abstract void Excute(Fsm fsm);
    }
}