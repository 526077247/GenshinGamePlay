using System;

namespace TaoTie
{
    public class I18NBridge
    {
        public static I18NBridge Instance { get; private set; } = new I18NBridge();

        public Action OnLanguageChangeEvt;
        public Func<string, string> GetValueByKey;

        /// <summary>
        /// 通过I18NKey获取多语言文本
        /// </summary>
        /// <param name="i18NKey"></param>
        /// <returns></returns>
        public string GetText(string i18NKey)
        {
            return GetValueByKey?.Invoke(i18NKey);
        }
        
    }
}