using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class VariableSet : IDisposable
    {
        private VariableSet _parent;
        private Dictionary<string, float> _varDict = new Dictionary<string, float>();
        
        public delegate void OnVariableChangeDelegate(string key,float value,float oldValue);
        
        public event OnVariableChangeDelegate onValueChange;

        public static VariableSet Create()
        {
            return ObjectPool.Instance.Fetch<VariableSet>();
        }
        

        /// <summary>
        /// 设置变量
        /// </summary>
        /// <param name="key">变量名</param>
        /// <param name="val">值</param>
        public void Set(string key, float val)
        {
            float old = Get(key);
            if (!SetInternal(key, val))
            {
                _varDict.Add(key, val);
            }
            onValueChange?.Invoke(key,val,old);
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <typeparam name="T">变量类型</typeparam>
        /// <param name="key">变量名</param>
        /// <returns>值</returns>
        public float Get(string key)
        {
            if (_varDict.TryGetValue(key, out var vb))
            {
                return vb;
            }
            if (_parent != null)
            {
                return _parent.Get(key);
            }
            return default;
        }
        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <typeparam name="T">变量类型</typeparam>
        /// <param name="key">变量名</param>
        /// <returns>值</returns>
        public bool TryGet(string key,out float res)
        {
            if (_varDict.TryGetValue(key, out res))
            {
                return true;
            }
            if (_parent != null)
            {
                return _parent.TryGet(key,out res);
            }

            res = default;
            return false;
        }
        /// <summary>
        /// 清理变量
        /// </summary>
        /// <param name="key">变量名</param>
        public void Remove(string key)
        {
            if (_varDict.TryGetValue(key, out var vb))
            {
                _varDict.Remove(key);
            }
        }

        /// <summary>
        /// 是否存在变量
        /// </summary>
        /// <param name="key">变量名</param>
        public bool Contain(string key)
        {
            return _varDict.ContainsKey(key);
        }

        /// <summary>
        /// 清理变量
        /// </summary>
        public void Clear()
        {
            _varDict.Clear();
        }

        public void SetParent(VariableSet varset)
        {
            _parent = varset;
        }

        public void Dispose()
        {
            Clear();
            onValueChange = null;
            _parent = null;
            ObjectPool.Instance.Recycle(this);
        }

        private bool SetInternal(string key, float val)
        {
            if (_varDict.TryGetValue(key, out var vb))
            {
                _varDict[key] = val;
            }
            return false;
        }

#if UNITY_EDITOR
        private System.Text.StringBuilder _builder = new System.Text.StringBuilder();
        public string DebugInfo()
        {
            _builder.Clear();
            using (var iter = _varDict.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    _builder.AppendFormat("{0}: {1}\n", iter.Current.Key, iter.Current.Value);
                }
            }
            return _builder.ToString();
        }
#endif
    }
}
