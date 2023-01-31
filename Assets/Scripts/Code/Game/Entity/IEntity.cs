using System;

namespace TaoTie
{
    public interface IEntityDestroy
    {
        public void Destroy();
    }

    public interface IEntity : IEntityDestroy
    {
        public void Init();

    }

    public interface IEntity<P1> : IEntityDestroy
    {
        public void Init(P1 p1);

    }

    public interface IEntity<P1, P2> : IEntityDestroy
    {
        public void Init(P1 p1, P2 p2);

    }

    public interface IEntity<P1, P2, P3> : IEntityDestroy
    {
        public void Init(P1 p1, P2 p2, P3 p3);

    }

    public interface IEntity<P1, P2, P3, P4> : IEntityDestroy
    {
        public void Init(P1 p1, P2 p2, P3 p3, P4 p4);

    }

    public interface IEntity<P1, P2, P3, P4, P5> : IEntityDestroy
    {
        public void Init(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);

    }
}