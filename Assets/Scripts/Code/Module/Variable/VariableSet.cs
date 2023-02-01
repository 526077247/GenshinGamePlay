using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class VariableSet : IDisposable
    {
        private VariableSet _parent;
        private Dictionary<string, IVariable> _varDict = new Dictionary<string, IVariable>();
        
        public delegate void OnVariableChangeDelegate<T>(string key,T value,T oldValue);
        
        public event OnVariableChangeDelegate<float> onFloatValueChange;
        public event OnVariableChangeDelegate<int> onIntValueChange;
        
        public static VariableSet Create()
        {
            return ObjectPool.Instance.Fetch<VariableSet>();
        }
        

        /// <summary>
        /// 设置变量
        /// </summary>
        /// <typeparam name="T">变量类型</typeparam>
        /// <param name="key">变量名</param>
        /// <param name="val">值</param>
        public void Set<T>(string key, T val)
        {
            T old = Get<T>(key);
            if (!SetInternal(key, val))
            {
                var variable = Variable<T>.Create();
                variable.value = val;
                _varDict.Add(key, variable);
            }
            if (onFloatValueChange != null && val is float f && old is float oldF)
            {
                onFloatValueChange.Invoke(key,f,oldF);
            }
            if (onIntValueChange != null && val is int i && old is int oldI)
            {
                onIntValueChange.Invoke(key,i,oldI);
            }
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <typeparam name="T">变量类型</typeparam>
        /// <param name="key">变量名</param>
        /// <returns>值</returns>
        public T Get<T>(string key)
        {
            if (_varDict.TryGetValue(key, out var vb))
            {
                Variable<T> vt = vb as Variable<T>;
                if (vt != null)
                {
                    return vt.value;
                }
                else
                {
                    Log.Error($"获取值{key}时，前后类型不一致，原类型{vb.GetValueType().FullName}，新类型{this.GetType().FullName}");
                }
            }
            if (_parent != null)
            {
                return _parent.Get<T>(key);
            }
            return default;
        }
        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <typeparam name="T">变量类型</typeparam>
        /// <param name="key">变量名</param>
        /// <returns>值</returns>
        public bool TryGet<T>(string key,out T res)
        {
            if (_varDict.TryGetValue(key, out var vb))
            {
                Variable<T> vt = vb as Variable<T>;
                if (vt != null)
                {
                    res = vt.value;
                    return true;
                }
            }
            if (_parent != null)
            {
                return _parent.TryGet<T>(key,out res);
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
                vb.Dispose();
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
            foreach (var item in _varDict)
            {
                item.Value.Dispose();
            }
            _varDict.Clear();
        }

        public void SetParent(VariableSet varset)
        {
            _parent = varset;
        }

        public void Dispose()
        {
            Clear();
            onFloatValueChange = null;
            onIntValueChange = null;
            _parent = null;
            ObjectPool.Instance.Recycle(this);
        }

        private bool SetInternal<T>(string key, T val)
        {
            if (_varDict.TryGetValue(key, out var vb))
            {
                Variable<T> vt = vb as Variable<T>;
                if (vt != null)
                {
                    vt.value = val;
                    return true;
                }
                else
                {
                    vb.Dispose();
                    _varDict.Remove(key);
                    Log.Error($"设置 {key} 时，类型不匹配，原类型{vb.GetValueType().FullName}，新类型{this.GetType().FullName}");
                }
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
                    _builder.AppendFormat("{0}: {1}\n", iter.Current.Key, iter.Current.Value.debugValue);
                }
            }
            return _builder.ToString();
        }
#endif
    }
}
