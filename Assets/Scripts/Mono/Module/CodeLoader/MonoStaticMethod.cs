using System;
using System.Reflection;

namespace TaoTie
{
    public class MonoStaticAction : IStaticAction
    {
        private Action method;
        public MonoStaticAction(Assembly assembly, string typeName, string methodName)
        {
            var methodInfo = assembly.GetType(typeName)?.GetMethod(methodName);
            if (methodInfo != null)
                this.method = (Action) Delegate.CreateDelegate(TypeInfo<Action>.Type, null, methodInfo);
            else
                Log.Error($"Not Found {typeName}.{methodName}");
        }

        public void Run()
        {
            this.method?.Invoke();
        }
    }
    
    public class MonoStaticFunc<T> : IStaticFunc<T>
    {
        private Func<T> method;
        public MonoStaticFunc(Assembly assembly, string typeName, string methodName)
        {
            var methodInfo = assembly.GetType(typeName)?.GetMethod(methodName);
            if (methodInfo != null)
                this.method = (Func<T>)Delegate.CreateDelegate(TypeInfo<Func<T>>.Type, null, methodInfo);
            else
                Log.Error($"Not Found {typeName}.{methodName}");
        }
        
        public T Run()
        {
            if (method != null)
            {
                return this.method();
            }
            return default;
        }
    }
}