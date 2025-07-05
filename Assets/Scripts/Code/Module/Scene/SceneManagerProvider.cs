namespace TaoTie
{
    public abstract class SceneManagerProvider
    {
        public abstract string GetName();
        public T GetManager<T>() where T :class,IManagerDestroy
        {
            return ManagerProvider.GetManager<T>(GetName());
        }
        public T RegisterManager<T>() where T :class,IManager
        {
            return ManagerProvider.RegisterManager<T>(GetName());
        }
        public T RegisterManager<T,P1>(P1 p1) where T :class,IManager<P1>
        {
            return ManagerProvider.RegisterManager<T,P1>(p1,GetName());
        }
        public T RegisterManager<T,P1,P2>(P1 p1,P2 p2) where T :class,IManager<P1,P2>
        {
            return ManagerProvider.RegisterManager<T,P1,P2>(p1,p2,GetName());
        }
        public T RegisterManager<T,P1,P2,P3>(P1 p1,P2 p2,P3 p3) where T :class,IManager<P1,P2,P3>
        {
            return ManagerProvider.RegisterManager<T,P1,P2,P3>(p1,p2,p3,GetName());
        }

        public void RemoveManager<T>()
        {
            ManagerProvider.RemoveManager<T>(GetName());
        }

    }
}