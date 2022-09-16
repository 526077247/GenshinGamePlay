using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TaoTie
{
    public class DynamicAtlas:IDisposable
    {
        private int m_Width, m_Height = 0;
        private int m_Padding = 4;
        // private int m_PackedWidth, m_PackedHeight = 0;

        private List<DynamicAtlasPage> m_PageList = new List<DynamicAtlasPage>();
        private List<GetTextureData> m_GetTextureTaskList = new List<GetTextureData>();
        private List<IntegerRectangle> m_WaitAddNewAreaList = new List<IntegerRectangle>();
        private Dictionary<string, SaveTextureData> m_UsingTexture = new Dictionary<string, SaveTextureData>();

        private Color32[] m_TempColor;
        public DynamicAtlas(int group)
        {
            int length = group;
            m_TempColor = new Color32[length * length];
            for (int i = 0; i < m_TempColor.Length; i++)
            {
                m_TempColor[i] = Color.clear;
            }

            m_Width = length;
            m_Height = length;
            CreateNewPage();
        }

        DynamicAtlasPage CreateNewPage()
        {
            var page = DynamicAtlasPage.OnCreate(m_PageList.Count, m_Width, m_Height, m_TempColor);
            m_PageList.Add(page);
            return page;
        }

        #region Public Func

        public void Clear()
        {
            foreach (var data in m_UsingTexture)
            {
                data.Value.Dispose();
            }
            m_UsingTexture.Clear();
            foreach (var data in m_PageList)
            {
                data.Dispose();
            }
            m_PageList.Clear();
            foreach (var data in m_GetTextureTaskList)
            {
                data.Dispose();
            }
            m_GetTextureTaskList.Clear();
            foreach (var data in m_WaitAddNewAreaList)
            {
                data.Dispose();
            }
            m_WaitAddNewAreaList.Clear();
        }

        public void SetTexture(string name, Texture texture)
        {
            if (texture == null)
            {
                Log.Info("Texture is Null");
                return;
            }

            if (texture.width > m_Width || texture.height > m_Height)
            {
                Log.Info("Texture is too big");
                return;
            }

            if (m_UsingTexture.ContainsKey(name))
            {
                return;
            }

            GetTextureData data = GetTextureData.Create();
            data.name = name;
            m_GetTextureTaskList.Add(data);

            OnRenderTexture(data.name, (Texture2D)texture);
        }

        public bool TryGetSprite(string name,out Sprite res)
        {
            if (m_UsingTexture.TryGetValue(name,out var data))
            {
                data.referenceCount++;
                res = m_UsingTexture[name].sprite;
                if (res == null)
                {
                    Texture2D tex2D = m_PageList[data.texIndex].texture;
                    res = Sprite.Create((Texture2D)tex2D, data.rect, new Vector2(data.rect.width * .5f, data.rect.height * .5f), 
                        100, 1, SpriteMeshType.Tight, new Vector4(0, 0, 0, 0));
                    m_UsingTexture[name].sprite = res;
                }
                return true;
            }

            res = null;
            return false;
        }

        /// <summary>
        /// Image组件用完之后
        /// </summary>
        /// <param name="name"></param>
        public bool RemoveTexture(string name, bool clearRange = false)
        {
            if (m_UsingTexture.ContainsKey(name))
            {
                SaveTextureData data = m_UsingTexture[name];
                data.referenceCount--;
                if (data.referenceCount <= 0)//引用计数0
                {
                    if (clearRange)
                    {
                        Log.Info("Remove Texture:" + name);
                        m_PageList[data.texIndex].RemoveTexture(data.rect);
                    }

                    m_UsingTexture.Remove(name);
                    data.Dispose();
                    return true;
                }
            }
            return false;
        }

        #endregion

        private void OnRenderTexture(string name, Texture2D texture2D)
        {
            if (texture2D == null)
            {
                for (int i = m_GetTextureTaskList.Count - 1; i >= 0; i--)
                {
                    GetTextureData task = m_GetTextureTaskList[i];
                    task.Dispose();
                    m_GetTextureTaskList.RemoveAt(i);
                }

                return;
            }

            int index = 0;
            IntegerRectangle useArea = InsertArea(texture2D.width, texture2D.height, out index);
            // Log.Info(name + ":texture2D:" + texture2D.width+"  "+texture2D.height);

            if (useArea == null)
            {
                Log.Error("No Area");
                return;
            }

            Rect uv = new Rect((useArea.x), (useArea.y), texture2D.width, texture2D.height);
            m_PageList[index].AddTexture(useArea.x, useArea.y, texture2D);

            SaveTextureData saveTextureData = SaveTextureData.Create();
            saveTextureData.texIndex = index;
            saveTextureData.rect = uv;
            m_UsingTexture[name] = saveTextureData;

            for (int i = m_GetTextureTaskList.Count - 1; i >= 0; i--)
            {
                GetTextureData task = m_GetTextureTaskList[i];
                if (task.name.Equals(name))
                {
                    m_UsingTexture[name].referenceCount++;
                }
                task.Dispose();
                m_GetTextureTaskList.RemoveAt(i);
            }
        }

        private IntegerRectangle InsertArea(int width, int height, out int index)
        {
            IntegerRectangle result = null;
            IntegerRectangle freeArea = null;
            DynamicAtlasPage page = null;
            for (int i = 0; i < m_PageList.Count; i++)
            {
                int fIndex = m_PageList[i].GetFreeAreaIndex(width, height, m_Padding);
                if (fIndex >= 0)
                {
                    page = m_PageList[i];
                    freeArea = page.freeAreasList[fIndex];
                    break;
                }
            }

            if (freeArea == null)
            {
                Log.Error("No Free Area----Create New Page");
                page = CreateNewPage();
                freeArea = page.freeAreasList[0];
            }

            result = IntegerRectangle.Create(freeArea.x, freeArea.y, width, height);
            GenerateNewFreeAreas(result, page);

            page.RemoveFreeArea(freeArea);
            index = page.index;
            return result;
        }

        private void GenerateNewFreeAreas(IntegerRectangle target, DynamicAtlasPage page)
        {
            int x = target.x;
            int y = target.y;
            int right = target.right + 1 + m_Padding;
            int top = target.top + 1 + m_Padding;

            IntegerRectangle targetWithPadding = null;
            if (m_Padding == 0)
                targetWithPadding = target;

            for (int i = page.freeAreasList.Count - 1; i >= 0; i--)
            {
                IntegerRectangle area = page.freeAreasList[i];
                if (!(x >= area.right || right <= area.x || y >= area.top || top <= area.y))
                {
                    if (targetWithPadding == null)
                        targetWithPadding = IntegerRectangle.Create(target.x, target.y, target.width + m_Padding, target.height + m_Padding);

                    GenerateDividedAreas(targetWithPadding, area, m_WaitAddNewAreaList);
                    IntegerRectangle topOfStack = page.freeAreasList.Pop();
                    if (i < page.freeAreasList.Count)
                    {
                        // Move the one on the top to the freed position
                        page.freeAreasList[i] = topOfStack;
                    }
                }
            }

            if (targetWithPadding != null && targetWithPadding != target)
                targetWithPadding.Dispose();

            FilterSelfSubAreas(m_WaitAddNewAreaList);
            while (m_WaitAddNewAreaList.Count > 0)
            {
                var free = m_WaitAddNewAreaList.Pop();
                page.AddFreeArea(free);
            }

            // if (target.right > m_PackedWidth)
            //     m_PackedWidth = target.right;

            // if (target.top > m_PackedHeight)
            //     m_PackedHeight = target.top;
        }



        private void GenerateDividedAreas(IntegerRectangle divider, IntegerRectangle area, List<IntegerRectangle> results)
        {
            int count = 0;

            int rightDelta = area.right - divider.right;
            if (rightDelta > 0)
            {
                results.Add(IntegerRectangle.Create(divider.right, area.y, rightDelta, area.height));
                count++;
            }

            int leftDelta = divider.x - area.x;
            if (leftDelta > 0)
            {
                results.Add(IntegerRectangle.Create(area.x, area.y, leftDelta, area.height));
                count++;
            }

            int bottomDelta = area.top - divider.top;
            if (bottomDelta > 0)
            {
                results.Add(IntegerRectangle.Create(area.x, divider.top, area.width, bottomDelta));
                count++;
            }

            int topDelta = divider.y - area.y;
            if (topDelta > 0)
            {
                results.Add(IntegerRectangle.Create(area.x, area.y, area.width, topDelta));
                count++;
            }

            if (count == 0 && (divider.width < area.width || divider.height < area.height))
            {
                // Only touching the area, store the area itself
                results.Add(area);

            }
            else
                area.Dispose();
        }

        private void FilterSelfSubAreas(List<IntegerRectangle> areas)
        {
            for (int i = areas.Count - 1; i >= 0; i--)
            {
                IntegerRectangle filtered = areas[i];
                for (int j = areas.Count - 1; j >= 0; j--)
                {
                    if (i != j)
                    {
                        IntegerRectangle area = areas[j];
                        if (filtered.x >= area.x && filtered.y >= area.y && filtered.right <= area.right && filtered.top <= area.top)
                        {
                            filtered.Dispose();
                            IntegerRectangle topOfStack = areas.Pop();
                            if (i < areas.Count)
                            {
                                // Move the one on the top to the freed position
                                areas[i] = topOfStack;
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Clear();
            m_UsingTexture = null;
            m_PageList = null;
            m_WaitAddNewAreaList = null;
            m_GetTextureTaskList = null;
            m_TempColor = null;
        }

    }
    static class ListExtension
    {
        static public T Pop<T>(this List<T> list)
        {
            int index = list.Count - 1;

            T r = list[index];
            list.RemoveAt(index);
            return r;
        }

    }

    public class DynamicAtlasPage:IDisposable
    {
        private int m_Index;
        private Texture2D m_Texture;
        private List<IntegerRectangle> m_FreeAreasList = new List<IntegerRectangle>();
        private int m_Width, m_Height;

        public int index => m_Index;
        public Texture2D texture => m_Texture;
        public List<IntegerRectangle> freeAreasList => m_FreeAreasList;

        private void SetData(int width, int height)
        {
            if(m_Texture!=null)UnityEngine.Object.Destroy(m_Texture);
            m_Width = width;
            m_Height = height;
            m_Texture = new Texture2D(width, height, DynamicAtlasConfig.kTextureFormat, false, true);
            m_Texture.filterMode = FilterMode.Bilinear;
        }

        public static DynamicAtlasPage OnCreate(int index, int width, int height, Color32[] tempColor)
        {
            var res = ObjectPool.Instance.Fetch(typeof(DynamicAtlasPage)) as DynamicAtlasPage;
            if (res.m_Width != width || res.m_Height == height)
            {
                res.SetData(width, height);
            }

            if (tempColor != null)
            {
                res.m_Texture.SetPixels32(0, 0, width, height, tempColor);
                res.m_Texture.Apply(false);
            }

            res.m_Texture.name = string.Format("DynamicAtlas-{0}*{1}-{2}", width, height, index);
            res.m_Index = index;
            var area = IntegerRectangle.Create(0, 0, res.m_Width, res.m_Height);
            res.m_FreeAreasList.Add(area);
            return res;
        }
        public void AddTexture(int posX, int posY, Texture2D srcTex)
        {
            //可以把一张贴图画到另一张贴图上
            Graphics.CopyTexture(srcTex, 0, 0, 0, 0, srcTex.width, srcTex.height, m_Texture, 0, 0, posX, posY);
        }

        public void RemoveTexture(Rect rect)
        {
            int width = (int)rect.width;
            int height = (int)rect.height;
            Color32[] colors = new Color32[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.clear;
            }

            m_Texture.SetPixels32((int)rect.x, (int)rect.y, width, height, colors);
            m_Texture.Apply();
        }
        
        /// <summary>
        /// 搜寻空白区域
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public int GetFreeAreaIndex(int width, int height, int padding)
        {
            if (width > m_Width || height > m_Height)
            {
                Log.Error("To Large Texture for atlas");
                return -1;
            }

            IntegerRectangle best = IntegerRectangle.Create(m_Width + 1, m_Height + 1, 0, 0);
            int index = -1;

            int paddedWidth = width + padding;
            int paddedHeight = height + padding;
    
            for (int i = m_FreeAreasList.Count - 1; i >= 0; i--)
            {
                IntegerRectangle free = m_FreeAreasList[i];

                // if (free.x < width || free.y < height)
                {
                    if (free.x < best.x && paddedWidth <= free.width && paddedHeight <= free.height)//如果这个Free大小可以容纳目标大小的话
                    {
                        index = i;
                        if ((paddedWidth == free.width && free.width <= free.height && free.right < m_Width) || (paddedHeight == free.height && free.height <= free.width))//如果这个区域正好可以放得下
                            break;
                        best = free;
                    }
                    else
                    {
                        // Outside the current packed area, no padding required
                        if (free.x < best.x && width <= free.width && height <= free.height)//如果不算padding距离也可以放得下的话，也可以放进去
                        {
                            index = i;
                            if ((width == free.width && free.width <= free.height && free.right < m_Width) || (height == free.height && free.height <= free.width))
                                break;

                            best = free;
                        }
                    }
                }
            }
            
            return index;
        }

        public void AddFreeArea(IntegerRectangle area)
        {
            m_FreeAreasList.Add(area);
        }

        public void RemoveFreeArea(IntegerRectangle area)
        {
            m_FreeAreasList.Remove(area);
            area.Dispose();
        }
        
        public void Dispose()
        {
            for (int i = 0; i < m_FreeAreasList.Count; i++)
            {
                m_FreeAreasList[i].Dispose();
            }
            m_FreeAreasList.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}