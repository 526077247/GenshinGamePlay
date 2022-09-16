using System;

namespace TaoTie
{
    public class I18NBridge
    {
        public static I18NBridge Instance { get; private set; } = new I18NBridge();

        public Action OnLanguageChangeEvt;
        public Func<string, string> GetValueByKey;

        public Func<int, string> GetValueById;

        /// <summary>
        /// 通过中文本获取多语言文本(还没实现,先用根据ID获取的重载)
        /// </summary>
        /// <param name="str">中文文本</param>
        /// <returns></returns>
        public string GetText(string str)
        {
            return GetValueByKey?.Invoke(str);
        }
        
    }
}