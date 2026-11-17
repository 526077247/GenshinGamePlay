using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 元素表的手写扩展。<see cref="ElementConfigCategory"/> / <see cref="ElementConfig"/> 由
    /// <c>Tools/ExcelExport</c> 从 <c>Excel/ElementConfig@cs.xlsx</c> 生成，本文件不要放会与生成代码重名的成员。
    ///
    /// 框架层不认识任何具体元素：元素有几种、叫什么、上限多少、谁克谁，
    /// 全部只在这张表里。想加元素或改关系，改 Excel 重导即可，不需要动代码。
    /// </summary>
    public partial class ElementConfigCategory
    {
        /// <summary>
        /// 取元素定义，<b>Id &lt;= 0 或表里没有都返回 null，且不写日志</b>。
        ///
        /// 不复用基类 <c>Get(id)</c>：那个查不到会 <c>Log.Error</c>，而"这次攻击没带元素"（Id=0）
        /// 是每次普攻的常态，用它会刷屏。
        /// </summary>
        public ElementConfig GetElement(int id)
        {
            if (id <= 0) return null;
            var all = GetAll();
            if (all == null) return null;
            all.TryGetValue(id, out var item);
            return item;
        }

        private int maxAttachId;

        /// <summary>
        /// 可附着元素的最大 Id，<see cref="ElementComponent"/> 用它决定槽位数组长度。
        ///
        /// <b>只统计能真正附着上去的行</b>（<c>MaxDurability &gt; 0</c>）：上限填 0 表示"该元素不可附着"，
        /// 若把它们算进来，每个实体都会为这些永远不会存元素量的槽位付内存和逐帧扫描。
        ///
        /// 表没就绪时返回 0 且不缓存 —— 配置加载完成前调到这里不会把"0"钉死。
        /// </summary>
        public int GetMaxAttachId()
        {
            if (maxAttachId > 0) return maxAttachId;

            var all = GetAll();
            if (all == null || all.Count == 0) return 0;

            int max = 0;
            foreach (var kv in all)
            {
                var element = kv.Value;
                if (element == null || element.MaxDurability <= 0) continue;
                if (element.Id > max) max = element.Id;
            }
            maxAttachId = max;
            return max;
        }
    }
}
