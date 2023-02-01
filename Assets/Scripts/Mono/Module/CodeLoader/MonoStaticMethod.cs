using System;
using System.Reflection;

namespace TaoTie
{
    public class MonoStaticAction : IStaticAction
    {
        private Action method;
        public MonoStaticAction(Assembly assembly, string typeName, string methodName)
        {
            var methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            this.method = (Action)Delegate.CreateDelegate(TypeInfo<Action>.Type, null, methodInfo);
        }

        public void Run()
        {
            this.method();
        }
    }
    
    public class MonoStaticFunc<T> : IStaticFunc<T>
    {
        private Func<T> method;
        public MonoStaticFunc(Assembly assembly, string typeName, string methodName)
        {
            var methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            this.method = (Func<T>)Delegate.CreateDelegate(TypeInfo<Func<T>>.Type, null, methodInfo);
        }
        
        public T Run()
        {
            return this.method();
        }
    }
}