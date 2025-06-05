using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class I18NManager : IManager
    {
        public event Action OnLanguageChangeEvt;

        public static I18NManager Instance;
        //语言类型枚举

        public LangType CurLangType { get; private set; }
        private Dictionary<int, string> i18nTextKeyDic;
        private bool addFonts = false;
        #region override

        public void Init()
        {
            Instance = this;
            I18NBridge.Instance.GetValueByKey = I18NGetText;
            var lang = CacheManager.Instance.GetInt(CacheKeys.CurLangType, -1);
            if (lang < 0)
            {
                this.CurLangType = Application.systemLanguage == SystemLanguage.Chinese ||
                                   Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                                   Application.systemLanguage == SystemLanguage.ChineseTraditional
                    ? LangType.Chinese
                    : LangType.English;
            }
            else
            {
                this.CurLangType = (LangType)lang;
            }
            this.i18nTextKeyDic = new Dictionary<int, string>();
            InitAsync().Coroutine();
#if !UNITY_WEBGL
            AddSystemFonts();
#endif
        }

        private async ETTask InitAsync()
        {
            var res = await ConfigManager.Instance.LoadOneConfig<I18NConfigCategory>(this.CurLangType.ToString());
            for (int i = 0; i <res.GetAllList().Count; i++)
            {
                var item = res.GetAllList()[i];
                this.i18nTextKeyDic.Add(item.Id, item.Value);
            }
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
        public string I18NGetText(I18NKey key)
        {
            if (!this.i18nTextKeyDic.TryGetValue((int)key, out var result))
            {
                Log.Error("多语言key未添加！ " + key);
                result = key.ToString();
                return result;
            }

            return result;
        }
        
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string I18NGetText(string key)
        {
            if (!I18NKey.TryParse(key,out I18NKey i18nKey) || !this.i18nTextKeyDic.TryGetValue((int)i18nKey, out var result))
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
        public string I18NGetParamText(I18NKey key, params object[] paras)
        {
            if (!this.i18nTextKeyDic.TryGetValue((int)key, out var value))
            {
                Log.Error("多语言key未添加！ " + key);
                return key.ToString();
            }

            if (paras != null)
                return string.Format(value, paras);
            else
                return value;
        }
        
        /// <summary>
        /// 取配置多语言
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string I18NGetText(II18NConfig config)
        {
            return config.GetI18NText(CurLangType);
        }
        
        /// <summary>
        /// 取配置多语言
        /// </summary>
        /// <param name="config"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string I18NGetText(II18NSwitchConfig config, int pos = 0)
        {
            return config.GetI18NText(CurLangType, pos);
        }

        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool I18NTryGetText(I18NKey key, out string result)
        {
            if (!this.i18nTextKeyDic.TryGetValue((int)key, out result))
            {
                Log.Info("多语言key未添加！ " + key);
                result = key.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 切换语言,外部接口
        /// </summary>
        /// <param name="langType"></param>
        public async ETTask SwitchLanguage(int langType)
        {
            //修改当前语言
            CacheManager.Instance.SetInt(CacheKeys.CurLangType, langType);
            this.CurLangType = (LangType)langType;
            var res = await ConfigManager.Instance.LoadOneConfig<I18NConfigCategory>(this.CurLangType.ToString());
            this.i18nTextKeyDic.Clear();
            for (int i = 0; i <res.GetAllList().Count; i++)
            {
                var item = res.GetAllList()[i];
                this.i18nTextKeyDic.Add(item.Id, item.Value);
            }

            I18NBridge.Instance.OnLanguageChangeEvt?.Invoke();
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
        
                
        #region 添加系统字体
#if !UNITY_WEBGL
        /// <summary>
        /// 需要就添加
        /// </summary>
        public void AddSystemFonts()
        {
             if(addFonts) return;
             addFonts = true;
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
            string[] fonts = new[] { "msyhl" };//微软雅黑细体
#elif UNITY_ANDROID
            string[] fonts = new[] {
                "notosanscjksc-regular",
                "notosanscjk-regular",
            };
#elif UNITY_IOS
            string[] fonts = new[] {
                "pingfang" // 注意内存占用70m+
            };
#else
            string[] fonts = Array.Empty<string>();
#endif
            TextMeshFontAssetManager.Instance.AddWithOSFont(fonts);
        }
#else
        public void AddSystemFonts(){}
#endif
        public void RemoveSystemFonts()
        {
#if UNITY_WEBGL
            Log.Error("WebGL不支持加载系统字体");
            return;
#endif
            if(!addFonts) return;
            addFonts = false;
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
            string[] fonts = new[] { "msyhl" };//微软雅黑细体
#elif UNITY_ANDROID
            string[] fonts = new[] {
                "notosanscjksc-regular",
                "notosanscjk-regular",
            };
#elif UNITY_IOS
            string[] fonts = new[] {
                "pingfang"// 注意内存占用70m+
            };
#else
            string[] fonts = Array.Empty<string>();
#endif
            TextMeshFontAssetManager.Instance.RemoveWithOSFont(fonts);
        }
        #endregion
    }
}