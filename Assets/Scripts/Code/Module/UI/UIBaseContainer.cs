using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaoTie
{
    /// <summary>
    /// UI容器，可以自定义UI侧的组件，并对组件的生命周期进行管理，不受Unity生命周期管理
    /// 所有UI相关扩展组件都应继承此类
    /// </summary>
    public abstract class UIBaseContainer
    {
        UIBaseContainer Parent;
        MultiDictionary<string, Type, UIBaseContainer> components;//[path]:[component_name:UIBaseContainer]
        int length;
        GameObject gameObject;
        Transform transform;
        string path;
        
        public void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
        public GameObject GetGameObject()
        {
            if (gameObject == null)
            {
                gameObject = GetTransform().gameObject;
            }

            return gameObject;
        }
        public void SetTransform(Transform transform)
        {
            this.transform = transform;
        }
        public Transform GetTransform()
        {
            ActivatingComponent();
            return transform;
        }
        Transform ParentTransform;
        Transform ActivatingComponent()
        {
            if (this.transform == null)
            {
                var pui = this.Parent;
                this.transform = this.GetParentTransform()?.Find(path);
                if (this.transform == null)
                {
                    Log.Error(this.Parent.GetType().Name+"路径错误:" + path);
                }
            }
            return this.transform;
        }
        Transform GetParentTransform()
        {
            if (this.ParentTransform == null)
            {
                var pui = Parent;
                if (pui == null)
                {
                    Log.Error("ParentTransform is null Path:" + path);
                }
                else
                {
                    pui.ActivatingComponent();
                    ParentTransform = pui.transform;
                }
            }
            return ParentTransform;
        }

        public int GetLength()
        {
            return length;
        }
        public void SetLength(int length)
        {
            this.length = length;
        }
        void AfterOnEnable()
        {
            Walk((component) =>
            {
                if(component is IOnEnable a) a.OnEnable();
                component.AfterOnEnable();
            });
        }

        void BeforeOnDisable()
        {
            Walk((component) =>
            {
                component.BeforeOnDisable();
                if(component is IOnDisable a) a.OnDisable();
            });
        }

        public void BeforeOnDestroy()
        {
            if(components==null) return;
            var keys1 = components.Keys.ToList();
            for (int i = keys1.Count - 1; i >= 0; i--)
            {
                if (components[keys1[i]] != null)
                {
                    var keys2 = components[keys1[i]].Keys.ToList();
                    for (int j = keys2.Count - 1; j >= 0; j--)
                    {
                        var component = components[keys1[i]][keys2[j]];
                        component.BeforeOnDestroy();
                        if (component is II18N i18n)
                            I18NManager.Instance.RemoveI18NEntity(i18n);
                        if(component is IOnDestroy a) a.OnDestroy();
                        
                    }
                }
            }
            this.SetLength(this.GetLength()-1);
            if (this.GetLength() <= 0)
            {
                if (string.IsNullOrEmpty(path))
                    this.Parent.InnerRemoveComponent(this, path);
                else
                    Log.Info("Close window here, type name: "+this.GetType().Name);
            }    
            else
                Log.Error("OnDestroy fail, length != 0");
            
        }

        //遍历：注意，这里是无序的
        void Walk(Action<UIBaseContainer> callback)
        {
            if(components==null) return;
            foreach (var item in this.components)
            {
                if (item.Value != null)
                {
                    foreach (var item2 in item.Value)
                    {
                        callback(item2.Value);
                    }
                }
            }
        }

        //记录Component
        void RecordUIComponent(string name, Type component_class, UIBaseContainer component)
        {
            if (components == null) components = new MultiDictionary<string, Type, UIBaseContainer>();
            if (this.components.ContainSubKey(name, component_class))
            {
                Log.Error("Aready exist component_class : " + component_class.Name);
                return;
            }
            this.components.Add(name,component_class,component);
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="name">游戏物体名称</param>
        public T AddComponentNotCreate<T>(string name) where T : UIBaseContainer
        {
            Type type = typeof(T);
            T component_inst = Activator.CreateInstance<T>();;
            component_inst.path = name;
            component_inst.Parent = this;
            this.RecordUIComponent(name, type, component_inst);
            this.SetLength(this.GetLength()+1);
            return component_inst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public T AddComponent<T>(string path = "") where T : UIBaseContainer
        {
            Type type = typeof(T);
            T component_inst = Activator.CreateInstance<T>();
            component_inst.path = path;
            component_inst.Parent = this;
            if(component_inst is IOnCreate a)
                a.OnCreate();
            if (component_inst is II18N i18n)
                I18NManager.Instance.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, component_inst);
            this.SetLength(this.GetLength() + 1);
            return component_inst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">相对路径</param>
        public T AddComponent<T, A>(string path, A a) where T : UIBaseContainer,IOnCreate<A>
        {
            Type type = typeof(T);
            T component_inst = Activator.CreateInstance<T>();;
            component_inst.path = path;
            component_inst.Parent = this;
            component_inst.OnCreate(a);
            if (component_inst is II18N i18n)
                I18NManager.Instance.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, component_inst);
            this.SetLength(this.GetLength() + 1);
            return component_inst;
        }
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public T AddComponent<T, A, B>( string path, A a, B b) where T : UIBaseContainer,IOnCreate<A,B>
        {
            Type type = typeof(T);
            T component_inst = Activator.CreateInstance<T>();;
            component_inst.path = path;
            component_inst.Parent = this;
            component_inst.OnCreate(a,b);
            if (component_inst is II18N i18n)
                I18NManager.Instance.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, component_inst);
            this.SetLength(this.GetLength() + 1);
            return component_inst;
        }
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public T AddComponent<T, A, B, C>(string path, A a, B b, C c) where T : UIBaseContainer,IOnCreate<A,B,C>
        {
            Type type = typeof(T);
            T component_inst = Activator.CreateInstance<T>();;
            component_inst.path = path;
            component_inst.Parent = this;
            component_inst.OnCreate(a,b,c);
            if (component_inst is II18N i18n)
                I18NManager.Instance.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, component_inst);
            this.SetLength(this.GetLength() + 1);
            return component_inst;
        }

        private void InnerSetActive(bool active)
        {
            if(GetGameObject()!=null&&gameObject.activeSelf!=active)
                gameObject.SetActive(active);
        }
        
        public void SetActive( bool active)
        {

            if (active)
            {
                (this as IOnEnable)?.OnEnable();
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable)?.OnDisable();
            }
        }

        public void SetActive<T>( bool active, T param1)
        {
            if (active)
            {
                (this as IOnEnable<T>)?.OnEnable(param1);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T>)?.OnDisable(param1);
            }
        }
        public void SetActive<T, P>( bool active, T param1, P param2)
        {
            if (active)
            {
                (this as IOnEnable<T,P>)?.OnEnable(param1,param2);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T,P>)?.OnDisable(param1,param2);
            }
        }
        public void SetActive<T, P, K>( bool active, T param1, P param2, K param3)
        {
            if (active)
            {
                (this as IOnEnable<T,P,K>)?.OnEnable(param1,param2,param3);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T,P,K>)?.OnDisable(param1,param2,param3);
            }
        }

        public void SetActive<T, P, K, V>( bool active, T param1, P param2, K param3, V param4)
        {
            if (active)
            {
                (this as IOnEnable<T,P,K,V>)?.OnEnable(param1,param2,param3,param4);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T,P,K,V>)?.OnDisable(param1,param2,param3,param4);
            }
        }
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T GetComponent<T>( string path = "") where T : UIBaseContainer
        {
            if (components == null) return null;
            Type type = typeof(T);
            if (this.components.TryGetValue(path,type,out var component))
            {
                return component as T;
            }
            return null;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        public void RemoveComponent<T>(string path = "") where T : UIBaseContainer
        {
            var component = this.GetComponent<T>(path);
            if (component != null)
            {
                component.BeforeOnDestroy();
                if (component is II18N i18n)
                    I18NManager.Instance.RemoveI18NEntity(i18n);
                (component as IOnDestroy)?.OnDestroy();
                this.components.Remove(path,typeof(T));
            }
        }

        /// <summary>
        /// 移除组件回调方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        void InnerRemoveComponent(UIBaseContainer component, string path)
        {
            if (component != null)
            {
                this.components.Remove(path,component.GetType());
                this.SetLength(this.GetLength()-1);
            }
        }
    }
}