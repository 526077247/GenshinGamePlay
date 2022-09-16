using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class LangType
    {
        public const int Chinese = 0;
        public const int English = 1;
    }
    public class I18NManager:IManager
    {
        public event Action OnLanguageChangeEvt;
        public static I18NManager Instance;
        //语言类型枚举
        
        public int curLangType { get; set; }
        public Dictionary<string, string> i18nTextKeyDic;
        // public Dictionary<string, string> i18nPathKeyDic;
        #region override
        
        public void Init()
        {
            Instance = this;
            I18NBridge.Instance.GetValueByKey = I18NGetText;
            OnLanguageChangeEvt += I18NBridge.Instance.OnLanguageChangeEvt;
            this.curLangType = PlayerPrefs.GetInt(CacheKeys.CurLangType, 0);
            // this.I18NEntity = new HashSet<II18N>();
            this.i18nTextKeyDic = new Dictionary<string, string>();
            // this.i18nPathKeyDic = new Dictionary<string, string>();
            
            for(int i=0;i<I18NConfigCategory.Instance.GetAllList().Count;i++)
            {
                var item = I18NConfigCategory.Instance.GetAllList()[i];
                switch (this.curLangType)
                {
                    case LangType.Chinese:
                        this.i18nTextKeyDic.Add(item.Key, item.Chinese);
                        break;
                    case LangType.English:
                        this.i18nTextKeyDic.Add(item.Key, item.English);
                        break;
                    default:
                        this.i18nTextKeyDic.Add(item.Key, item.Chinese);
                        break;
                }
            }
            // for(int i=0;i<I18NImgConfigCategory.Instance.GetAllList().Count;i++)
            // {
            //     var item = I18NImgConfigCategory.Instance.GetAllList()[i];
            //     switch (this.curLangType)
            //     {
            //         case LangType.Chinese:
            //             this.i18nPathKeyDic.Add(item.Key, item.Chinese);
            //             break;
            //         case LangType.English:
            //             this.i18nPathKeyDic.Add(item.Key, item.English);
            //             break;
            //         default:
            //             this.i18nPathKeyDic.Add(item.Key, item.Chinese);
            //             break;
            //     }
            // }
        }

        public void Destroy()
        {
            Instance = null;
            this.i18nTextKeyDic.Clear();
            this.i18nTextKeyDic = null;
            // this.i18nPathKeyDic.Clear();
            // this.i18nPathKeyDic = null;
        }

        #endregion
        
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string I18NGetText( string key)
        {
            if (!this.i18nTextKeyDic.TryGetValue(key, out var result))
            {
                Log.Error("多语言key未添加！ "+key);
                result = key;
                return result;
            }
            return result;
        }
        /// <summary>
        /// 根据key取多语言取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public string I18NGetParamText( string key, params object[] paras)
        {
            if (!this.i18nTextKeyDic.TryGetValue(key, out var value))
            {
                Log.Error("多语言key未添加！ "+key);
                return key;
            }
            if (paras != null)
                return string.Format(value, paras);
            else
                return value;
        }
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool I18NTryGetText( string key, out string result)
        {
            if (!this.i18nTextKeyDic.TryGetValue(key, out result))
            {
                Log.Info("多语言key未添加！ "+key);
                result = key;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        // public bool I18NTryGetPath( string key, out string result)
        // {
        //     if (!this.i18nPathKeyDic.TryGetValue(key, out result))
        //     {
        //         Log.Info("多语言图片key未添加！ "+key);
        //         result = key;
        //         return false;
        //     }
        //     return true;
        // }
        /// <summary>
        /// 切换语言,外部接口
        /// </summary>
        /// <param name="langType"></param>
        public void SwitchLanguage( int langType)
        {
            //修改当前语言
            PlayerPrefs.SetInt(CacheKeys.CurLangType, langType);
            this.curLangType = langType;
            this.i18nTextKeyDic.Clear();
            for(int i=0;i<I18NConfigCategory.Instance.GetAllList().Count;i++)
            {
                var item = I18NConfigCategory.Instance.GetAllList()[i];
                switch (this.curLangType)
                {
                    case LangType.Chinese:
                        this.i18nTextKeyDic.Add(item.Key, item.Chinese);
                        break;
                    case LangType.English:
                        this.i18nTextKeyDic.Add(item.Key, item.English);
                        break;
                    default:
                        this.i18nTextKeyDic.Add(item.Key, item.Chinese);
                        break;
                }
            }
            // this.i18nPathKeyDic.Clear();
            // for(int i=0;i<I18NImgConfigCategory.Instance.GetAllList().Count;i++)
            // {
            //     var item = I18NImgConfigCategory.Instance.GetAllList()[i];
            //     switch (this.curLangType)
            //     {
            //         case LangType.Chinese:
            //             this.i18nPathKeyDic.Add(item.Key, item.Chinese);
            //             break;
            //         case LangType.English:
            //             this.i18nPathKeyDic.Add(item.Key, item.English);
            //             break;
            //         default:
            //             this.i18nPathKeyDic.Add(item.Key, item.Chinese);
            //             break;
            //     }
            // }
            
            OnLanguageChangeEvt?.Invoke();
        }

        public void RegisterI18NEntity(II18N entity)
        {
            OnLanguageChangeEvt += entity.OnLanguageChange;
        }
        
        public void RemoveI18NEntity(II18N entity)
        {
            OnLanguageChangeEvt -= entity.OnLanguageChange;
        }
    }
}