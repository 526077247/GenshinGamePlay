using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;

namespace TaoTie
{
    public class ImageLoaderManager:IManager
    {
        private static class SpriteType
        {
            public static int Sprite = 0;
            public static int SpriteAtlas = 1;
            public static int DynSpriteAtlas = 2;
        }
        private class SpriteValue
        {
            public Sprite Asset;
            public int RefCount;
        }

        private class SpriteAtlasValue
        {
            public Dictionary<string, SpriteValue> SubAsset;
            public SpriteAtlas Asset;
            public int RefCount;
        }
        
        private class OnlineImage
        {
            public OnlineImage(Texture2D texture2D,Sprite sprite,int refCount)
            {
                Texture2D = texture2D;
                Sprite = sprite;
                RefCount = refCount;
            }
            public Texture2D Texture2D;
            public Sprite Sprite;
            public int RefCount;
        }
        
        private const string ATLAS_KEY = "/Atlas/";
        private const string DYN_ATLAS_KEY="/DynamicAtlas/";
        public static ImageLoaderManager Instance { get; private set; }

        private LruCache<string, SpriteValue> cacheSingleSprite;

        private LruCache<string, SpriteAtlasValue> cacheSpriteAtlas;

        private Dictionary<string, DynamicAtlas> cacheDynamicAtlas;
        
        private Dictionary<string, OnlineImage> cacheOnlineImage;


        #region overrride

        public void Init()
        {
            Instance = this;
            this.cacheSingleSprite = new LruCache<string, SpriteValue>();
            this.cacheSpriteAtlas = new LruCache<string, SpriteAtlasValue>();
            this.cacheDynamicAtlas = new Dictionary<string, DynamicAtlas>();
            cacheOnlineImage = new Dictionary<string, OnlineImage>();
            this.InitSingleSpriteCache(this.cacheSingleSprite);
            this.InitSpriteAtlasCache(this.cacheSpriteAtlas);
            PreLoad().Coroutine();
        }

        public void Destroy()
        {
            this.Clear();
            this.cacheOnlineImage = null;
            this.cacheDynamicAtlas = null;
            this.cacheSingleSprite = null;
            this.cacheSpriteAtlas = null;
            Instance = null;
        }

        #endregion
        
        async ETTask PreLoad()
        {
            for (int i = 0; i < 2; i++)//看情况提前预加载，加载会卡顿
            {
                await TimerManager.Instance.WaitAsync(1);
                var temp = DynamicAtlasPage.OnCreate(i, DynamicAtlasGroup.Size_2048, DynamicAtlasGroup.Size_2048,null);
                temp.Dispose();
            }
        }
        void InitSpriteAtlasCache(LruCache<string, SpriteAtlasValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteAtlasValue value) => {
                return value.RefCount == 0;
            });

            cache.SetPopCallback((key, value) => {
                var subasset = value.SubAsset;
                foreach (var item in subasset)
                {
                    UnityEngine.Object.Destroy(item.Value.Asset);
                    item.Value.Asset = null;
                    item.Value.RefCount = 0;
                }
                ResourcesManager.Instance.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.RefCount = 0;
            });
        }
        
        void InitSingleSpriteCache(LruCache<string, SpriteValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteValue value) => {
                return value.RefCount == 0;
            });
            cache.SetPopCallback((key, value) => {
                ResourcesManager.Instance.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.RefCount = 0;
            });
        }
        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ETTask LoadImageTask(string imagePath, Action<Sprite> callback = null)
        {
            ETTask task = ETTask.Create();
            this.LoadImageAsync(imagePath, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }
        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadImageAsync(string imagePath, Action<Sprite> callback = null)
        {
            Sprite res = null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, imagePath.GetHashCode());
                this.GetSpriteLoadInfoByPath(imagePath, out int assetType, out string assetAddress,
                    out string subAssetName);
                if (assetType == SpriteType.Sprite)
                {
                    res = await this.LoadSingleImageAsyncInternal( assetAddress,callback);
                }
                else if (assetType == SpriteType.DynSpriteAtlas)
                {
                    res = await this.LoadDynSpriteImageAsyncInternal(assetAddress, callback);
                }
                else
                {
                    res = await this.LoadSpriteImageAsyncInternal(assetAddress, subAssetName, callback);
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
            this.GetSpriteLoadInfoByPath(imagePath, out int assetType, out string assetAddress, out string subAssetName);

            if (assetType == SpriteType.SpriteAtlas)
            {
                if (this.cacheSpriteAtlas.TryOnlyGet(assetAddress, out SpriteAtlasValue value))
                {
                    if (value.RefCount > 0)
                    {
                        var subasset = value.SubAsset;
                        if (subasset.ContainsKey(subAssetName))
                        {
                            subasset[subAssetName].RefCount --;
                            if (subasset[subAssetName].RefCount <= 0)
                            {
                                GameObject.Destroy(subasset[subAssetName].Asset);
                                subasset.Remove(subAssetName);
                            }
                            value.RefCount --;
                        }
                    }
                }
            }
            else if (assetType == SpriteType.DynSpriteAtlas)
            {
                var index = assetAddress.IndexOf(DYN_ATLAS_KEY);
                var path = assetAddress.Substring(0, index);
                if (this.cacheDynamicAtlas.TryGetValue(path, out var value))
                {
                    value.RemoveTexture(imagePath, true);
                }
            }
            else
            {
                if (this.cacheSingleSprite.TryOnlyGet(imagePath, out SpriteValue value))
                {
                    if (value.RefCount > 0)
                    {
                        value.RefCount --;
                    }
                }
            }

        }


        /// <summary>
        /// 异步加载图集： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadAtlasImageAsync(string atlasPath, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, atlasPath.GetHashCode());
                res = await this.LoadAtlasImageAsyncInternal(atlasPath, null, callback);
                callback?.Invoke(res);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        
        /// <summary>
        /// 异步加载图片： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadSingleImageAsync(string atlasPath, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, atlasPath.GetHashCode());
                res = await this.LoadSingleImageAsyncInternal(atlasPath, callback);
                callback?.Invoke(res);

            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        #region private

        async ETTask<Sprite> LoadAtlasImageAsyncInternal(string assetAddress, string subAssetName, Action<Sprite> callback = null)
        {
            var cacheCls = this.cacheSpriteAtlas;
            if (cacheCls.TryGet(assetAddress, out var valueC))
            {
                if (valueC.Asset == null)
                {
                    cacheCls.Remove(assetAddress);
                }
                else
                {
                    valueC.RefCount ++;
                    if (valueC.SubAsset.TryGetValue(subAssetName, out var result))
                    {
                        valueC.SubAsset[subAssetName].RefCount ++;
                        callback?.Invoke(result.Asset);
                        return result.Asset;
                    }
                    else
                    {
                        var sp = valueC.Asset.GetSprite(subAssetName);
                        if (sp == null)
                        {
                            Log.Error("image not found:" + subAssetName);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (valueC.SubAsset == null)
                            valueC.SubAsset = new Dictionary<string, SpriteValue>();
                        valueC.SubAsset[subAssetName] = new SpriteValue { Asset = sp, RefCount = 1 };
                        callback?.Invoke(sp);
                        return sp;
                    }
                }
            }
            var asset = await ResourcesManager.Instance.LoadAsync<SpriteAtlas>(assetAddress);
            if (asset != null)
            {
                if (cacheCls.TryGet(assetAddress, out var value))
                {
                    value.RefCount ++;
                }
                else
                {
                    value = new SpriteAtlasValue() { Asset = asset , RefCount = 1 };
                    cacheCls.Set(assetAddress, value);
                }
                if (value.SubAsset.TryGetValue(subAssetName, out var result))
                {
                    value.SubAsset[subAssetName].RefCount++;
                    callback?.Invoke(result.Asset);
                    return result.Asset;
                }
                else
                {
                    var sp = value.Asset.GetSprite(subAssetName);
                    if (sp == null)
                    {
                        Log.Error("image not found:" + subAssetName);
                        callback?.Invoke(null);
                        return null;
                    }
                    if (value.SubAsset == null)
                        value.SubAsset = new Dictionary<string, SpriteValue>();
                    value.SubAsset[subAssetName] = new SpriteValue { Asset = sp, RefCount = 1 };
                    callback?.Invoke(sp);
                    return sp;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        async ETTask<Sprite> LoadSingleImageAsyncInternal(string assetAddress, Action<Sprite> callback = null)
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
                    callback?.Invoke(valueC.Asset);
                    return valueC.Asset;
                }
            }
            var asset = await ResourcesManager.Instance.LoadAsync<Sprite>(assetAddress);
            if (asset != null)
            {
                if (cacheCls.TryGet(assetAddress, out var value))
                {
                    value.RefCount ++;
                }
                else
                {
                    value = new SpriteValue() { Asset = asset, RefCount = 1 };
                    cacheCls.Set(assetAddress, value);
                    callback?.Invoke(value.Asset);
                    return value.Asset;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        void GetSpriteLoadInfoByPath(string imagePath, out int assetType, out string assetAddress, out string subAssetName)
        {
            assetAddress = imagePath;
            subAssetName = "";
            var index = imagePath.IndexOf(ATLAS_KEY);
            if (index < 0)
            {
                //没有找到/atlas/，则是散图
                index = imagePath.IndexOf(DYN_ATLAS_KEY);
                if (index < 0)
                {
                    //是散图
                    assetType = SpriteType.Sprite;
                    return;
                }
                else
                {
                    //是动态图集
                    assetType = SpriteType.DynSpriteAtlas;
                    return;
                }
            }
            assetType = SpriteType.SpriteAtlas;
            var substr = imagePath.Substring(index + ATLAS_KEY.Length);
            var subIndex = substr.IndexOf('/');
            string atlasPath;
            string spriteName;
            if (subIndex >= 0)
            {
                //有子目录
                var prefix = imagePath.Substring(0, index+1);
                var name = substr.Substring(0, subIndex);
                atlasPath = string.Format("{0}{1}.spriteatlas", prefix, "Atlas_" + name);
                var dotIndex = substr.LastIndexOf(".");
                var lastSlashIndex = substr.LastIndexOf('/');
                spriteName = substr.Substring(lastSlashIndex+1, dotIndex - lastSlashIndex-1);
            }
            else
            {
                var prefix = imagePath.Substring(0, index + 1);

                atlasPath = prefix + "Atlas.spriteatlas";


                var dotIndex = substr.LastIndexOf(".");

                spriteName = substr.Substring(0, dotIndex);
            }
            assetAddress = atlasPath;
            subAssetName = spriteName;
        }

        async ETTask<Sprite> LoadSpriteImageAsyncInternal(
             string assetAddress, string subAssetName, Action<Sprite> callback)
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
                    Sprite result;
                    var subassetList = valueC.SubAsset;
                    if (subassetList.ContainsKey(subAssetName))
                    {
                        result = subassetList[subAssetName].Asset;
                        subassetList[subAssetName].RefCount ++;
                        valueC.RefCount++;
                    }
                    else
                    {
                        result = valueC.Asset.GetSprite(subAssetName);
                        if (result == null)
                        {
                            Log.Error("image not found:" + assetAddress + "" + subAssetName);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (valueC.SubAsset == null)
                            valueC.SubAsset = new Dictionary<string, SpriteValue>();
                        valueC.SubAsset[subAssetName] = new SpriteValue { Asset = result, RefCount = 1 };
                        valueC.RefCount++;
                    }
                    callback?.Invoke(result);
                    return result;
                }
            }
            var asset = await ResourcesManager.Instance.LoadAsync<SpriteAtlas>(assetAddress);
            if (asset != null)
            {
                Sprite result;
                var sa = asset;
                if (cacheCls.TryGet(assetAddress, out valueC))
                {
                    var subasset_list = valueC.SubAsset;
                    if (subasset_list.ContainsKey(subAssetName))
                    {
                        result = subasset_list[subAssetName].Asset;
                        subasset_list[subAssetName].RefCount = subasset_list[subAssetName].RefCount + 1;
                    }
                    else
                    {
                        result = valueC.Asset.GetSprite(subAssetName);
                        if (result == null)
                        {
                            Log.Error("image not found:" + assetAddress + "" + subAssetName);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (valueC.SubAsset == null)
                            valueC.SubAsset = new Dictionary<string, SpriteValue>();
                        valueC.SubAsset[subAssetName] = new SpriteValue { Asset = result, RefCount = 1 };
                    }
                }
                else
                {
                    valueC = new SpriteAtlasValue { Asset = sa, SubAsset = new Dictionary<string, SpriteValue>(), RefCount = 1 };
                    result = valueC.Asset.GetSprite(subAssetName);
                    if (result == null)
                    {
                        Log.Error("image not found:" + assetAddress + "" + subAssetName);
                        callback?.Invoke(null);
                        return null;
                    }
                    if (valueC.SubAsset == null)
                        valueC.SubAsset = new Dictionary<string, SpriteValue>();
                    valueC.SubAsset[subAssetName] = new SpriteValue { Asset = result, RefCount = 1 };
                    cacheCls.Set(assetAddress, valueC);
                }
                callback?.Invoke(result);
                return result;
            }
            callback?.Invoke(null);
            return null;
        }

        async ETTask<Sprite> LoadDynSpriteImageAsyncInternal(
             string assetAddress, Action<Sprite> callback)
        {
            Dictionary<string, DynamicAtlas> cacheCls = this.cacheDynamicAtlas;
            var index = assetAddress.IndexOf(DYN_ATLAS_KEY);
            var path = assetAddress.Substring(0, index);
            if (cacheCls.TryGetValue(path, out DynamicAtlas valueC))
            {
                Sprite result;
                if (valueC.TryGetSprite(assetAddress,out result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    var texture = await ResourcesManager.Instance.LoadAsync<Texture>(assetAddress);
                    if (texture == null)
                    {
                        Log.Error("image not found:" + assetAddress);
                        callback?.Invoke(null);
                        return null;
                    }
                    valueC.SetTexture(assetAddress,texture);
                    ResourcesManager.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                    if (valueC.TryGetSprite(assetAddress,out  result))
                    {
                        callback?.Invoke(result);
                        return result;
                    }
                    Log.Error("image not found:" + assetAddress );
                    callback?.Invoke(null);
                    return null;
                }
            }
            else
            {
                // Log.Info(this.Id +" "+ cacheCls.Count);
                Log.Info("CreateNewDynamicAtlas  ||"+path+"||");
                valueC = new DynamicAtlas(DynamicAtlasGroup.Size_2048);
                cacheCls.Add(path,valueC);
                // Log.Info(this.Id +" "+ cacheCls.Count);
                var texture = await ResourcesManager.Instance.LoadAsync<Texture>(assetAddress);
                if (texture == null)
                {
                    Log.Error("image not found:" + assetAddress );
                    callback?.Invoke(null);
                    return null;
                }
                valueC.SetTexture(assetAddress,texture);
                ResourcesManager.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                if (valueC.TryGetSprite(assetAddress,out var result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                Log.Error("image not found:" + assetAddress);
                callback?.Invoke(null);
                return null;
            }
        }
        #endregion

        public void Cleanup()
        {
            Log.Info("ImageLoaderManager Cleanup");
            this.cacheSingleSprite.CleanUp();
            this.cacheSpriteAtlas.CleanUp();
        }

        public void Clear()
        {
            foreach ((string key,var value) in this.cacheSpriteAtlas)
            {
                if (value.SubAsset != null)
                    foreach (var item in value.SubAsset)
                    {
                        GameObject.Destroy(item.Value.Asset);
                    }
                ResourcesManager.Instance?.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.SubAsset = null;
                value.RefCount = 0;
            }
            this.cacheSpriteAtlas.Clear();

            foreach ((string key, var value) in this.cacheSingleSprite)
            {
                ResourcesManager.Instance?.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.RefCount = 0;
            }
            this.cacheSingleSprite.Clear();

            foreach (var item in this.cacheDynamicAtlas)
            {
                item.Value.Dispose();
            }
            this.cacheDynamicAtlas.Clear();

            foreach (var item in this.cacheOnlineImage)
            {
                GameObject.Destroy(item.Value.Texture2D);
            }
            this.cacheOnlineImage.Clear();
            Log.Info("ImageLoaderManager Clear");
        }


        #region Online

        public void ReleaseOnlineImage(string url,bool clear = true)
        {
            if (this.cacheOnlineImage.TryGetValue(url,out var data))
            {
                data.RefCount--;
                if (clear && data.RefCount <= 0)
                {
                    if (data.Sprite != null)
                    {
                        GameObject.Destroy(data.Sprite);
                    }
                    GameObject.Destroy(data.Texture2D);
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
                            if (img.Sprite != null)
                            {
                                GameObject.Destroy(img.Sprite);
                            }
                            GameObject.Destroy(img.Texture2D);
                            cacheOnlineImage.Remove(temp[i]);
                        }
                    } 
                    
                }
            }
        }

        public async ETTask<Sprite> GetOnlineSprite(string url,int tryCount = 3)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());
                if (this.cacheOnlineImage.TryGetValue(url, out var data))
                {
                    data.RefCount++;
                    if (data.Sprite == null)
                    {
                        data.Sprite = Sprite.Create(data.Texture2D, new Rect(0, 0, data.Texture2D.width, data.Texture2D.height),
                            new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                    }
                    return data.Sprite;
                }

                var texture = await HttpManager.Instance.HttpGetImageOnline(url, true);
                if (texture != null) //本地已经存在
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                    this.cacheOnlineImage.Add(url, new OnlineImage(texture, sprite, 1));
                    return sprite;
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
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                        var bytes = texture.EncodeToPNG();
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            File.WriteAllBytes(HttpManager.Instance.LocalImage(url), bytes);
                        });
                        this.cacheOnlineImage.Add(url, new OnlineImage(texture, sprite, 1));
                        return sprite;
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
        
         public async ETTask<Texture2D> GetOnlineTexture(string url,int tryCount = 3)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());
                if (this.cacheOnlineImage.TryGetValue(url, out var data))
                {
                    data.RefCount++;
                    return data.Texture2D;
                }

                var texture = await HttpManager.Instance.HttpGetImageOnline(url, true);
                if (texture != null) //本地已经存在
                {
                    this.cacheOnlineImage.Add(url, new OnlineImage(texture, null, 1));
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
                        var bytes = texture.EncodeToPNG();
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            File.WriteAllBytes(HttpManager.Instance.LocalImage(url), bytes);
                        });
                        // GameObject.Destroy(texture);
                        this.cacheOnlineImage.Add(url, new OnlineImage(texture, null, 1));
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

        #endregion
    }
}