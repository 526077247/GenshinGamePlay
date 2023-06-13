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

    public class I18NManager : IManager
    {
        public event Action OnLanguageChangeEvt;

        public static I18NManager Instance;
        //语言类型枚举

        public int curLangType { get; set; }
        public Dictionary<string, string> i18nTextKeyDic;

        #region override

        public void Init()
        {
            Instance = this;
            I18NBridge.Instance.GetValueByKey = I18NGetText;
            this.curLangType = PlayerPrefs.GetInt(CacheKeys.CurLangType, -1);
            if (this.curLangType < 0)
            {
                this.curLangType = Application.systemLanguage == SystemLanguage.Chinese
                    ? LangType.Chinese
                    : LangType.English;
            }
            this.i18nTextKeyDic = new Dictionary<string, string>();

            for (int i = 0; i < I18NConfigCategory.Instance.GetAllList().Count; i++)
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
            
            ConfigManager.Instance.ReleaseConfig<I18NConfigCategory>();
            I18NConfigCategory.Instance = null;
        }

        public void Destroy()
        {
            OnLanguageChangeEvt = null;
            Instance = null;
            this.i18nTextKeyDic.Clear();
            this.i18nTextKeyDic = null;
        }

        #endregion

        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string I18NGetText(string key)
        {
            if (!this.i18nTextKeyDic.TryGetValue(key, out var result))
            {
                Log.Error("多语言key未添加！ " + key);
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
        public string I18NGetParamText(string key, params object[] paras)
        {
            if (!this.i18nTextKeyDic.TryGetValue(key, out var value))
            {
                Log.Error("多语言key未添加！ " + key);
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
        public bool I18NTryGetText(string key, out string result)
        {
            if (!this.i18nTextKeyDic.TryGetValue(key, out result))
            {
                Log.Info("多语言key未添加！ " + key);
                result = key;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 切换语言,外部接口
        /// </summary>
        /// <param name="langType"></param>
        public void SwitchLanguage(int langType)
        {
            ConfigManager.Instance.LoadOneConfig<I18NConfigCategory>();
            //修改当前语言
            PlayerPrefs.SetInt(CacheKeys.CurLangType, langType);
            this.curLangType = langType;
            this.i18nTextKeyDic.Clear();
            for (int i = 0; i < I18NConfigCategory.Instance.GetAllList().Count; i++)
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

            I18NBridge.Instance.OnLanguageChangeEvt?.Invoke();
            OnLanguageChangeEvt?.Invoke();
            
                        
            ConfigManager.Instance.ReleaseConfig<I18NConfigCategory>();
            I18NConfigCategory.Instance = null;
        }

        public void RegisterI18NEntity(II18N entity)
        {
            OnLanguageChangeEvt += entity.OnLanguageChange;
        }

        public void RemoveI18NEntity(II18N entity)
        {
            OnLanguageChangeEvt -= entity.OnLanguageChange;
        }
        
                
        #region 添加系统字体

        /// <summary>
        /// 需要就添加
        /// </summary>
        public static void AddSystemFonts()
        {
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
            string[] fonts = new[] { "STSONG" };
#elif UNITY_ANDROID
            string[] fonts = new[] {
                "NotoSansDevanagari-Regular",//天城体梵文
                "NotoSansThai-Regular",        //泰文
                "NotoSerifHebrew-Regular",     //希伯来文
                "NotoSansSymbols-Regular-Subsetted",  //符号
                "NotoSansCJK-Regular"          //中日韩
            };
#elif UNITY_IOS
            string[] fonts = new[] {
                "DevanagariSangamMN",  //天城体梵文
                "AppleSDGothicNeo",    //韩文，包含日文，部分中文
                "Thonburi",            //泰文
                "ArialHB"              //希伯来文
            };
#else
            string[] fonts = new string[0];
#endif
            TextMeshFontAssetManager.Instance.AddWithOSFont(fonts);
        }

        #endregion
    }
}