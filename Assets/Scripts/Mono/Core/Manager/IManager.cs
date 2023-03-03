namespace TaoTie
{
    public interface IManagerDestroy
    {
        public void Destroy();
    }
    public interface IManager:IManagerDestroy
    {
        public void Init();

    }
    
    public interface IManager<P1>:IManagerDestroy
    {
        public void Init(P1 p1);

    }
    
    public interface IManager<P1,P2>:IManagerDestroy
    {
        public void Init(P1 sceneGroups,P2 p2);

    }
    
    public interface IManager<P1,P2,P3>:IManagerDestroy
    {
        public void Init(P1 p1,P2 p2,P3 p3);

    }
}