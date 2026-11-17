using System.Collections.Generic;

namespace TaoTie
{
    /// <summary>
    /// 元素反应表的手写扩展。<see cref="ElementReactionCategory"/> / <see cref="ElementReaction"/> 由
    /// <c>Tools/ExcelExport</c> 从 <c>Excel/ElementReaction@cs.xlsx</c> 生成，本文件不要放会与生成代码重名的成员。
    ///
    /// 表里只写「哪两个元素会反应」，<b>连消耗多少都不用配</b> —— 反应按数值等量抵消：
    /// 双方各扣 <c>min(两边的量)</c>（见 <see cref="ElementComponent"/>）。
    /// <b>反应产生什么效果一律不在这张表里</b>：伤害 / 增伤 / 上 buff 由配置层挂的元素反应 Ability 自己算
    /// （见 <see cref="ConfigElementReactionMixin"/>）。
    /// </summary>
    public partial class ElementReactionCategory
    {
        /// <summary>无序对 → 反应行。由全表两两登记出来，A/B 谁在前都命中同一条。</summary>
        private Dictionary<long, ElementReaction> pairMap;

        /// <summary>
        /// 查这两个元素会不会反应；不会 / 表没就绪都返回 null。
        ///
        /// 只登记 <c>ElementA != ElementB</c> 且都 &gt; 0 的行：自己和自己反应没有意义，
        /// 0 是"无元素"的占位，配进去只会制造意外。
        /// </summary>
        public ElementReaction GetReaction(int elementA, int elementB)
        {
            if (elementA <= 0 || elementB <= 0 || elementA == elementB) return null;

            EnsurePairMap();
            if (pairMap == null) return null;
            pairMap.TryGetValue(PairKey(elementA, elementB), out var reaction);
            return reaction;
        }

        private void EnsurePairMap()
        {
            if (pairMap != null) return;

            var all = GetAllList();
            if (all == null || all.Count == 0) return; // 表还没就绪，下次调用再建

            pairMap = new Dictionary<long, ElementReaction>();
            for (int i = 0; i < all.Count; i++)
            {
                var reaction = all[i];
                if (reaction == null) continue;

                int a = reaction.ElementA;
                int b = reaction.ElementB;
                if (a <= 0 || b <= 0 || a == b) continue;

                long key = PairKey(a, b);
                // 同一对元素被两行声明时以先出现的为准；不覆盖，配置错误才能稳定复现
                if (!pairMap.ContainsKey(key)) pairMap[key] = reaction;
            }
        }

        private static long PairKey(int a, int b)
        {
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            return ((long)lo << 32) | (uint)hi;
        }
    }
}
