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
        UIBaseContainer parent;
        UnOrderDoubleKeyDictionary<string, Type, UIBaseContainer> components; //[path]:[component_name:UIBaseContainer]
        int length;
        GameObject gameObject;
        Transform transform;
        Transform parentTransform;
        string path;
        private long timerId;
        
        public bool ActiveSelf { get; private set; }
        
        public void SetGameObject(GameObject obj)
        {
            this.gameObject = obj;
        }

        public GameObject GetGameObject()
        {
            if (gameObject == null)
            {
                gameObject = GetTransform().gameObject;
            }

            return gameObject;
        }

        public void SetTransform(Transform trans)
        {
            this.transform = trans;
        }

        public Transform GetTransform()
        {
            ActivatingComponent();
            return transform;
        }
        public RectTransform GetRectTransform()
        {
            ActivatingComponent();
            return transform as RectTransform;
        }


        Transform ActivatingComponent()
        {
            if (this.transform == null)
            {
                var pTrans = this.GetParentTransform();
                if (pTrans != null)
                {
                    var rc = pTrans.GetComponent<ReferenceCollector>();
                    if (rc != null)
                    {
                        transform = rc.Get<Transform>(path);
                    }

                    if (this.transform == null)
                    {
                        this.transform = pTrans.Find(path);
#if UNITY_EDITOR
                        if (transform != null && !string.IsNullOrEmpty(path) && rc != null)
                        {
                            rc.Add(path, transform);
                        }
#endif
                    }
                }
                if (this.transform == null)
                {
                    Log.Error(this.parent.GetType().Name + "路径错误:" + path);
                }
            }

            return this.transform;
        }

        Transform GetParentTransform()
        {
            if (this.parentTransform == null)
            {
                var pui = parent;
                if (pui == null)
                {
                    Log.Error("ParentTransform is null Path:" + path);
                }
                else
                {
                    pui.ActivatingComponent();
                    parentTransform = pui.transform;
                }
            }

            return parentTransform;
        }

        void AfterOnEnable()
        {
            Walk((component) =>
            {
                if (component is IOnEnable a) a.OnEnable();
                component.ActiveSelf = true;
                component.AfterOnEnable();
            });
            if (this is IUpdate)
            {
                TimerManager.Instance.Remove(ref timerId);
                timerId = TimerManager.Instance.NewFrameTimer(TimerType.ComponentUpdate, this);
            }
        }

        void BeforeOnDisable()
        {
            if (this is IUpdate)
            {
                TimerManager.Instance.Remove(ref timerId);
            }
            Walk((component) =>
            {
                component.BeforeOnDisable();
                if (component is IOnDisable a) a.OnDisable();
            });
        }

        public void BeforeOnDestroy()
        {
            if (this is IUpdate)
            {
                TimerManager.Instance.Remove(ref timerId);
            }
            if (components != null)
            {
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
                                I18NManager.Instance?.RemoveI18NEntity(i18n);
                            if (component is IOnDestroy a) a.OnDestroy();
                        }
                    }
                }
            }

            length--;
            if (this.length <= 0)
            {
                if (this.parent != null && path != null)
                    this.parent.InnerRemoveComponent(this, path);
                else
                    Log.Info("Close window here, type name: " + this.GetType().Name);
            }
            else
                Log.Error("OnDestroy fail, length != 0");
        }

        /// <summary>
        /// 遍历：注意，这里是无序的
        /// </summary>
        /// <param name="callback"></param>
        void Walk(Action<UIBaseContainer> callback)
        {
            if (components == null) return;
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

        /// <summary>
        /// 记录Component
        /// </summary>
        /// <param name="name"></param>
        /// <param name="componentClass"></param>
        /// <param name="component"></param>
        void RecordUIComponent(string name, Type componentClass, UIBaseContainer component)
        {
            if (components == null) components = new UnOrderDoubleKeyDictionary<string, Type, UIBaseContainer>();
            if (this.components.ContainSubKey(name, componentClass))
            {
                Log.Error("Already exist component_class : " + componentClass.Name);
                return;
            }

            this.components.Add(name, componentClass, component);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                component.ActivatingComponent();
            }
#endif
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="name">游戏物体名称</param>
        public T AddComponentNotCreate<T>(string name) where T : UIBaseContainer
        {
            Type type = TypeInfo<T>.Type;
            T componentInst = Activator.CreateInstance<T>();
 
            componentInst.path = name;
            componentInst.parent = this;
            this.RecordUIComponent(name, type, componentInst);
            length++;
            return componentInst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public T AddComponent<T>(string path = "") where T : UIBaseContainer
        {
            Type type = TypeInfo<T>.Type;
            T componentInst = Activator.CreateInstance<T>();
            componentInst.path = path;
            componentInst.parent = this;
            if (componentInst is IOnCreate a)
                a.OnCreate();
            if (componentInst is II18N i18n)
                I18NManager.Instance?.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, componentInst);
            length++;
            return componentInst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">相对路径</param>
        public T AddComponent<T, A>(string path, A a) where T : UIBaseContainer, IOnCreate<A>
        {
            Type type = TypeInfo<T>.Type;
            T componentInst = Activator.CreateInstance<T>();

            componentInst.path = path;
            componentInst.parent = this;
            componentInst.OnCreate(a);
            if (componentInst is II18N i18n)
                I18NManager.Instance?.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, componentInst);
            length++;
            return componentInst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public T AddComponent<T, A, B>(string path, A a, B b) where T : UIBaseContainer, IOnCreate<A, B>
        {
            Type type = TypeInfo<T>.Type;
            T componentInst = Activator.CreateInstance<T>();

            componentInst.path = path;
            componentInst.parent = this;
            componentInst.OnCreate(a, b);
            if (componentInst is II18N i18n)
                I18NManager.Instance?.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, componentInst);
            length++;
            return componentInst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public T AddComponent<T, A, B, C>(string path, A a, B b, C c) where T : UIBaseContainer, IOnCreate<A, B, C>
        {
            Type type = TypeInfo<T>.Type;
            T componentInst = Activator.CreateInstance<T>();
            
            componentInst.path = path;
            componentInst.parent = this;
            componentInst.OnCreate(a, b, c);
            if (componentInst is II18N i18n)
                I18NManager.Instance?.RegisterI18NEntity(i18n);
            this.RecordUIComponent(path, type, componentInst);
            length++;
            return componentInst;
        }

        private void InnerSetActive(bool active)
        {
            ActiveSelf = active;
            if (GetGameObject() != null && gameObject.activeSelf != active)
                gameObject.SetActive(active);
        }

        public void SetActive(bool active)
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

        public void SetActive<T>(bool active, T param1)
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

        public void SetActive<T, P>(bool active, T param1, P param2)
        {
            if (active)
            {
                (this as IOnEnable<T, P>)?.OnEnable(param1, param2);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T, P>)?.OnDisable(param1, param2);
            }
        }

        public void SetActive<T, P, K>(bool active, T param1, P param2, K param3)
        {
            if (active)
            {
                (this as IOnEnable<T, P, K>)?.OnEnable(param1, param2, param3);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T, P, K>)?.OnDisable(param1, param2, param3);
            }
        }

        public void SetActive<T, P, K, V>(bool active, T param1, P param2, K param3, V param4)
        {
            if (active)
            {
                (this as IOnEnable<T, P, K, V>)?.OnEnable(param1, param2, param3, param4);
                this.AfterOnEnable();
                InnerSetActive(active);
            }
            else
            {
                InnerSetActive(active);
                this.BeforeOnDisable();
                (this as IOnDisable<T, P, K, V>)?.OnDisable(param1, param2, param3, param4);
            }
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T GetComponent<T>(string path = "") where T : UIBaseContainer
        {
            if (components == null) return null;
            Type type = TypeInfo<T>.Type;
            if (this.components.TryGetValue(path, type, out var component))
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
                component.BeforeOnDisable();
                (component as IOnDisable)?.OnDisable();
                component.BeforeOnDestroy();
                if (component is II18N i18n)
                    I18NManager.Instance?.RemoveI18NEntity(i18n);
                (component as IOnDestroy)?.OnDestroy();
                this.components.Remove(path, TypeInfo<T>.Type);
            }
        }
        
        
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        public void RemoveAllComponent(string path = "")
        {
            if (components == null) return;
            if (this.components.TryGetDic(path, out var dic))
            {
                var list = dic.Values.ToArray();
                foreach (var component in list)
                {
                    if (component != null)
                    {
                        component.BeforeOnDisable();
                        (component as IOnDisable)?.OnDisable();
                        component.BeforeOnDestroy();
                        if (component is II18N i18n)
                            I18NManager.Instance?.RemoveI18NEntity(i18n);
                        (component as IOnDestroy)?.OnDestroy();
                    }
                }
            }
            components.Remove(path);
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
                this.components.Remove(path, component.GetType());
                length--;
            }
        }
    }
}