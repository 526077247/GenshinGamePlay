using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;

namespace TaoTie
{
    /// <summary>
    /// 图片加载系统，会自动识别图集，对调用方而言只需要知道路径，仅支持加载Sprite类型的图片或网络图片
    /// Texture类型的通过ResourcesManager自己加载管理
    /// </summary>
    public class ImageLoaderManager : IManager
    {
        private static class SpriteType
        {
            public static int Sprite = 0;
            public static int SpriteAtlas = 1;
        }

        private class SpriteValue
        {
            public Sprite Asset;
            public Texture2D Texture;
            public int RefCount;
        }

        private class SpriteAtlasValue
        {
            public Dictionary<string, SpriteValue> SubAsset;
            public SpriteAtlas Asset;

            public int RefCount
            {
                get
                {
                    var res = 0;
                    foreach (var item in SubAsset)
                    {
                        res += item.Value.RefCount;
                    }

                    return res;
                }
            }
        }

        private const string ATLAS_KEY = "/Atlas/";
        public static ImageLoaderManager Instance { get; private set; }

        private LruCache<string, SpriteValue> cacheSingleSprite;

        private LruCache<string, SpriteAtlasValue> cacheSpriteAtlas;

        private Dictionary<string, SpriteValue> cacheOnlineImage;


        #region overrride

        public void Init()
        {
            Instance = this;
            this.cacheSingleSprite = new LruCache<string, SpriteValue>();
            this.cacheSpriteAtlas = new LruCache<string, SpriteAtlasValue>();
            cacheOnlineImage = new Dictionary<string, SpriteValue>();
            this.InitSingleSpriteCache(this.cacheSingleSprite);
            this.InitSpriteAtlasCache(this.cacheSpriteAtlas);
            // PreLoad().Coroutine();
        }

        public void Destroy()
        {
            this.Clear();
            this.cacheOnlineImage = null;
            this.cacheSingleSprite = null;
            this.cacheSpriteAtlas = null;
            Instance = null;
        }

        #endregion

        void InitSpriteAtlasCache(LruCache<string, SpriteAtlasValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteAtlasValue value) => { return value.RefCount == 0; });

            cache.SetPopCallback((key, value) =>
            {
                var subasset = value.SubAsset;
                foreach (var item in subasset)
                {
                    UnityEngine.Object.Destroy(item.Value.Asset);
                    item.Value.Asset = null;
                    if (item.Value.Texture != null)
                    {
                        GameObject.Destroy(item.Value.Texture);
                        item.Value.Texture = null;
                    }

                    item.Value.RefCount = 0;
                }

                ResourcesManager.Instance.ReleaseAsset(value.Asset);
                value.Asset = null;
            });
        }

        void InitSingleSpriteCache(LruCache<string, SpriteValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteValue value) => { return value.RefCount == 0; });
            cache.SetPopCallback((key, value) =>
            {
                ResourcesManager.Instance.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.Texture = null;
                value.RefCount = 0;
            });
        }

        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ETTask LoadSpriteTask(string imagePath, Action<Sprite> callback = null)
        {
            ETTask task = ETTask.Create();
            this.LoadSpriteAsync(imagePath, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }

        /// <summary>
        /// 异步加载图片精灵 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadSpriteAsync(string imagePath, Action<Sprite> callback = null)
        {
            Sprite res = null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, imagePath.GetHashCode());
                this.GetSpriteLoadInfoByPath(imagePath, out int assetType, out string assetAddress,
                    out string subAssetName);
                if (assetType != SpriteType.SpriteAtlas)
                {
                    var sv = await this.LoadSingleImageAsyncInternal(assetAddress);
                    callback?.Invoke(sv.Asset);
                    return sv.Asset;
                }
                else
                {
                    var sv = await this.LoadSpriteImageAsyncInternal(assetAddress, subAssetName);
                    callback?.Invoke(sv.Asset);
                    return sv.Asset;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                coroutineLock?.Dispose();
            }

            return res;
        }

        /// <summary>
        /// 异步加载图片纹理 会自动识别图集并切分生成纹理：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Texture> LoadTextureAsync(string imagePath, Action<Texture> callback = null)
        {
            Texture res = null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, imagePath.GetHashCode());
                this.GetSpriteLoadInfoByPath(imagePath, out int assetType, out string assetAddress,
                    out string subAssetName);
                if (assetType != SpriteType.SpriteAtlas)
                {
                    var sv = await this.LoadSingleImageAsyncInternal(assetAddress);
                    callback?.Invoke(sv.Texture);
                    return sv.Texture;
                }
                else
                {
                    var sv = await this.LoadSpriteImageAsyncInternal(assetAddress, subAssetName);
                    if (sv.Texture == null)
                    {
                        if (!sv.Asset.texture.isReadable)
                        {
                            Log.Error("不建议加载图集中的图片, 如果需要这么做, 将对应SpriteAtlas的Read/Write属性打开(运行占用内存翻倍)");
                            callback?.Invoke(sv.Asset.texture);
                            return sv.Asset.texture;
                        }

                        var targetTex = new Texture2D((int) sv.Asset.rect.width, (int) sv.Asset.rect.height);
                        var pixels = sv.Asset.texture.GetPixels(
                            (int) sv.Asset.textureRect.x,
                            (int) sv.Asset.textureRect.y,
                            (int) sv.Asset.textureRect.width,
                            (int) sv.Asset.textureRect.height);
                        targetTex.SetPixels(pixels);
                        targetTex.Apply();
                        sv.Texture = targetTex;
                    }

                    callback?.Invoke(sv.Texture);
                    return sv.Texture;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                coroutineLock?.Dispose();
            }

            return res;
        }

        /// <summary>
        /// 释放图片
        /// </summary>
        /// <param name="imagePath"></param>
        public void ReleaseImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;
            this.GetSpriteLoadInfoByPath(imagePath, out int assetType, out string assetAddress,
                out string subAssetName);

            if (assetType == SpriteType.SpriteAtlas)
            {
                if (this.cacheSpriteAtlas.TryOnlyGet(assetAddress, out SpriteAtlasValue value))
                {
                    if (value.RefCount > 0)
                    {
                        var subasset = value.SubAsset;
                        if (subasset.ContainsKey(subAssetName))
                        {
                            subasset[subAssetName].RefCount--;
                            if (subasset[subAssetName].RefCount <= 0)
                            {
                                GameObject.Destroy(subasset[subAssetName].Asset);
                                subasset.Remove(subAssetName);
                            }
                        }
                    }
                }
            }
            else
            {
                if (this.cacheSingleSprite.TryOnlyGet(imagePath, out SpriteValue value))
                {
                    if (value.RefCount > 0)
                    {
                        value.RefCount--;
                    }
                }
            }

        }
        
        public void Cleanup()
        {
            Log.Info("ImageLoaderManager Cleanup");
            this.cacheSingleSprite.CleanUp();
            this.cacheSpriteAtlas.CleanUp();
        }

        public void Clear()
        {
            foreach ((string key, var value) in this.cacheSpriteAtlas)
            {
                if (value.SubAsset != null)
                    foreach (var item in value.SubAsset)
                    {
                        GameObject.Destroy(item.Value.Asset);
                        if (item.Value.Texture != null)
                        {
                            GameObject.Destroy(item.Value.Texture);
                            item.Value.Texture = null;
                        }
                    }

                ResourcesManager.Instance?.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.SubAsset = null;
            }

            this.cacheSpriteAtlas.Clear();

            foreach ((string key, var value) in this.cacheSingleSprite)
            {
                ResourcesManager.Instance?.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.Texture = null;
                value.RefCount = 0;
            }

            this.cacheSingleSprite.Clear();

            foreach (var item in this.cacheOnlineImage)
            {
                if(item.Value.Asset != null) GameObject.Destroy(item.Value.Asset);
                GameObject.Destroy(item.Value.Texture);
            }

            this.cacheOnlineImage.Clear();
            Log.Info("ImageLoaderManager Clear");
        }
        #region private
        async ETTask<SpriteValue> LoadSingleImageAsyncInternal(string assetAddress)
        {
            var cacheCls = this.cacheSingleSprite;
            if (cacheCls.TryGet(assetAddress, out var valueC))
            {
                if (valueC.Asset == null)
                {
                    cacheCls.Remove(assetAddress);
                }
                else
                {
                    valueC.RefCount = valueC.RefCount + 1;
                    return valueC;
                }
            }

            var asset = await ResourcesManager.Instance.LoadAsync<Sprite>(assetAddress);
            if (asset != null)
            {
                if (cacheCls.TryGet(assetAddress, out var value))
                {
                    value.RefCount++;
                }
                else
                {
                    value = new SpriteValue() {Asset = asset, Texture = asset.texture, RefCount = 1};
                    cacheCls.Set(assetAddress, value);
                    return value;
                }
            }
            else
            {
                Log.Error("图片精灵不存在！请检查图片设置！\n" + assetAddress);
            }
            return null;
        }
        
        async ETTask<SpriteValue> LoadSpriteImageAsyncInternal(string assetAddress, string subAssetName)
        {
            LruCache<string, SpriteAtlasValue> cacheCls = this.cacheSpriteAtlas;
            if (cacheCls.TryGet(assetAddress, out SpriteAtlasValue valueC))
            {
                if (valueC.Asset == null)
                {
                    cacheCls.Remove(assetAddress);
                }
                else
                {
                    SpriteValue result;
                    var subassetList = valueC.SubAsset;
                    if (subassetList.ContainsKey(subAssetName))
                    {
                        result = subassetList[subAssetName];
                        subassetList[subAssetName].RefCount++;
                    }
                    else
                    {
                        var sprite = valueC.Asset.GetSprite(subAssetName);
                        if (sprite == null)
                        {
                            Log.Error("image not found:" + assetAddress + "" + subAssetName);
                            return null;
                        }

                        if (valueC.SubAsset == null)
                            valueC.SubAsset = new Dictionary<string, SpriteValue>();
                        result = valueC.SubAsset[subAssetName] = new SpriteValue {Asset = sprite, RefCount = 1};
                    }

                    return result;
                }
            }

            var asset = await ResourcesManager.Instance.LoadAsync<SpriteAtlas>(assetAddress);
            if (asset != null)
            {
                SpriteValue result;
                var sa = asset;
                if (cacheCls.TryGet(assetAddress, out valueC))
                {
                    var subasset_list = valueC.SubAsset;
                    if (subasset_list.ContainsKey(subAssetName))
                    {
                        result = subasset_list[subAssetName];
                        subasset_list[subAssetName].RefCount = subasset_list[subAssetName].RefCount + 1;
                    }
                    else
                    {
                        var sprite = valueC.Asset.GetSprite(subAssetName);
                        if (sprite == null)
                        {
                            Log.Error("image not found:" + assetAddress + "" + subAssetName);
                            return null;
                        }

                        if (valueC.SubAsset == null)
                            valueC.SubAsset = new Dictionary<string, SpriteValue>();
                        result = valueC.SubAsset[subAssetName] = new SpriteValue {Asset = sprite, RefCount = 1};
                    }
                }
                else
                {
                    valueC = new SpriteAtlasValue {Asset = sa, SubAsset = new Dictionary<string, SpriteValue>()};
                    var sprite = valueC.Asset.GetSprite(subAssetName);
                    if (sprite == null)
                    {
                        Log.Error("image not found:" + assetAddress + "" + subAssetName);
                        return null;
                    }

                    if (valueC.SubAsset == null)
                        valueC.SubAsset = new Dictionary<string, SpriteValue>();
                    result = valueC.SubAsset[subAssetName] = new SpriteValue {Asset = sprite, RefCount = 1};
                    cacheCls.Set(assetAddress, valueC);
                }

                return result;
            }

            return null;
        }

        void GetSpriteLoadInfoByPath(string imagePath, out int assetType, out string assetAddress,
            out string subAssetName)
        {
            assetAddress = imagePath;
            subAssetName = "";
            var index = imagePath.IndexOf(ATLAS_KEY);
            if (index < 0)
            {
                //没有找到/atlas/，则是散图
                assetType = SpriteType.Sprite;
                return;
            }

            assetType = SpriteType.SpriteAtlas;
            var prefix = imagePath.Substring(0, index + 1);
            var substr = imagePath.Substring(index + ATLAS_KEY.Length);
            var subIndex = substr.IndexOf('/');
            string atlasPath;
            string spriteName;
            if (subIndex >= 0)
            {
                //有子目录
                var name = substr.Substring(0, subIndex);
                atlasPath = string.Format("{0}{1}.spriteatlas", prefix, "Atlas_" + name);
                var dotIndex = substr.LastIndexOf(".");
                var lastSlashIndex = substr.LastIndexOf('/');
                spriteName = substr.Substring(lastSlashIndex + 1, dotIndex - lastSlashIndex - 1);
            }
            else
            {
                atlasPath = prefix + "Atlas.spriteatlas";
                var dotIndex = substr.LastIndexOf(".");
                spriteName = substr.Substring(0, dotIndex);
            }

            assetAddress = atlasPath;
            subAssetName = spriteName;
        }
        #endregion
        
        #region Online
        public async ETTask<Sprite> GetOnlineSprite(string url, int tryCount = 3)
        {
            await GetOnlineTexture(url, tryCount);
            if (this.cacheOnlineImage.TryGetValue(url, out var data))
            {
                if (data.Asset == null)
                {
                    data.Asset = Sprite.Create(data.Texture, new Rect(0, 0, data.Texture.width, data.Texture.height),
                        new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                }
                return data.Asset;
            }
            return null;
        }

        public async ETTask<Texture2D> GetOnlineTexture(string url, int tryCount = 3)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());
                if (this.cacheOnlineImage.TryGetValue(url, out var data))
                {
                    data.RefCount++;
                    return data.Texture;
                }

                var texture = await HttpManager.Instance.HttpGetImageOnline(url, true);
                if (texture != null) //本地已经存在
                {
                    this.cacheOnlineImage.Add(url, new SpriteValue{Texture = texture,RefCount = 1});
                    return texture;
                }
                else
                {
                    for (int i = 0; i < tryCount; i++)
                    {
                        texture = await HttpManager.Instance.HttpGetImageOnline(url, false);
                        if (texture != null) break;
                    }

                    if (texture != null)
                    {
#if !UNITY_WEBGL || UNITY_EDITOR
                        var bytes = texture.EncodeToPNG();
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            File.WriteAllBytes(HttpManager.Instance.LocalFile(url), bytes);
                        });
#endif
                        this.cacheOnlineImage.Add(url, new SpriteValue{Texture = texture,RefCount = 1});
                        return texture;
                    }
                    else
                    {
                        Log.Error("网络无资源 " + url);
                    }
                }
            }
            finally
            {
                coroutineLock?.Dispose();
            }

            return null;
        }
        
        public void ReleaseOnlineImage(string url, bool clear = true)
        {
            if (this.cacheOnlineImage.TryGetValue(url, out var data))
            {
                data.RefCount--;
                if (clear && data.RefCount <= 0)
                {
                    if (data.Asset != null)
                    {
                        GameObject.Destroy(data.Asset);
                    }

                    GameObject.Destroy(data.Texture);
                    cacheOnlineImage.Remove(url);
                }

                if (this.cacheOnlineImage.Count > 10)
                {
                    using (ListComponent<string> temp = ListComponent<string>.Create())
                    {
                        foreach (var item in cacheOnlineImage)
                        {
                            if (item.Value.RefCount == 0)
                            {
                                temp.Add(item.Key);
                            }
                        }

                        for (int i = 0; i < temp.Count; i++)
                        {
                            var img = cacheOnlineImage[temp[i]];
                            if (img.Asset != null)
                            {
                                GameObject.Destroy(img.Asset);
                            }

                            GameObject.Destroy(img.Texture);
                            cacheOnlineImage.Remove(temp[i]);
                        }
                    }

                }
            }
        }

        #endregion
    }
}