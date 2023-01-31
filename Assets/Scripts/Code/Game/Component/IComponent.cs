using System;

namespace TaoTie
{
    public interface IComponentDestroy
    {
        public void Destroy();
    }

    public interface IComponent : IComponentDestroy
    {
        public void Init();

    }

    public interface IComponent<P1> : IComponentDestroy
    {
        public void Init(P1 p1);

    }

    public interface IComponent<P1, P2> : IComponentDestroy
    {
        public void Init(P1 p1, P2 p2);

    }

    public interface IComponent<P1, P2, P3> : IComponentDestroy
    {
        public void Init(P1 p1, P2 p2, P3 p3);

    }
}