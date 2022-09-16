using System;


namespace TaoTie
{

    public interface ISupportInitialize
    {
        void BeginInit();
        void EndInit();
    }


    public abstract class ProtoObject: Object, ISupportInitialize
    {
        public object Clone()
        {
            byte[] bytes = ProtobufHelper.ToBytes(this);
            return ProtobufHelper.FromBytes(this.GetType(), bytes, 0, bytes.Length);
        }
        
        public virtual void BeginInit()
        {
        }
        
        public virtual void EndInit()
        {
        }

        public virtual void AfterEndInit()
        {
        }
    }
    
    public abstract class DisposeObject: Object, IDisposable, ISupportInitialize
    {
        public virtual void Dispose()
        {
        }
        
        public virtual void BeginInit()
        {
        }
        
        public virtual void EndInit()
        {
        }
        

        public override string ToString()
        {
            return this.GetType().Name;

        }

    }
}