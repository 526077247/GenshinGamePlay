using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System;
namespace TaoTie
{
    
    public static class SpriteType
    {
        public static int Sprite = 0;
        public static int SpriteAtlas = 1;
        public static int DynSpriteAtlas = 2;
    }
    public class SpriteValue
    {
        public Sprite asset;
        public int ref_count;
    }

    public class SpriteAtlasValue
    {
        public Dictionary<string, SpriteValue> subasset;
        public SpriteAtlas asset;
        public int ref_count;
    }
    public class ImageLoaderManager:IManager
    {
        public readonly string ATLAS_KEY = "/Atlas/";
        public readonly string DYN_ATLAS_KEY="/DynamicAtlas/";
        public static ImageLoaderManager Instance { get; private set; }

        public LruCache<string, SpriteValue> m_cacheSingleSprite;

        public LruCache<string, SpriteAtlasValue> m_cacheSpriteAtlas;

        public Dictionary<string, DynamicAtlas> m_cacheDynamicAtlas;


        #region overrride

        public void Init()
        {
            Instance = this;
            this.m_cacheSingleSprite = new LruCache<string, SpriteValue>();
            this.m_cacheSpriteAtlas = new LruCache<string, SpriteAtlasValue>();
            this.m_cacheDynamicAtlas = new Dictionary<string, DynamicAtlas>();
            this.InitSingleSpriteCache(this.m_cacheSingleSprite);
            this.InitSpriteAtlasCache(this.m_cacheSpriteAtlas);
            PreLoad().Coroutine();
        }

        public void Destroy()
        {
            this.Clear();
            this.m_cacheDynamicAtlas = null;
            this.m_cacheSingleSprite = null;
            this.m_cacheSpriteAtlas = null;
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
                return value.ref_count == 0;
            });

            cache.SetPopCallback((key, value) => {
                var subasset = value.subasset;
                foreach (var item in subasset)
                {
                    UnityEngine.Object.Destroy(item.Value.asset);
                    item.Value.asset = null;
                    item.Value.ref_count = 0;
                }
                ResourcesManager.Instance.ReleaseAsset(value.asset);
                value.asset = null;
                value.ref_count = 0;
            });
        }
        
        void InitSingleSpriteCache(LruCache<string, SpriteValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteValue value) => {
                return value.ref_count == 0;
            });
            cache.SetPopCallback((key, value) => {
                ResourcesManager.Instance.ReleaseAsset(value.asset);
                value.asset = null;
                value.ref_count = 0;
            });
        }
        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="image_path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ETTask LoadImageTask(string image_path, Action<Sprite> callback = null)
        {
            ETTask task = ETTask.Create();
            this.LoadImageAsync(image_path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }
        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="image_path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadImageAsync(string image_path, Action<Sprite> callback = null)
        {
            Sprite res = null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, image_path.GetHashCode());
                this.__GetSpriteLoadInfoByPath(image_path, out int asset_type, out string asset_address,
                    out string subasset_name);
                if (asset_type == SpriteType.Sprite)
                {
                    res = await this.__LoadSingleImageAsyncInternal( asset_address,callback);
                }
                else if (asset_type == SpriteType.DynSpriteAtlas)
                {
                    res = await this.__LoadDynSpriteImageAsyncInternal(asset_address, callback);
                }
                else
                {
                    res = await this.__LoadSpriteImageAsyncInternal(asset_address, subasset_name, callback);
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
        /// <param name="image_path"></param>
        public void ReleaseImage(string image_path)
        {
            if (string.IsNullOrEmpty(image_path))
                return;
            this.__GetSpriteLoadInfoByPath(image_path, out int asset_type, out string asset_address, out string subasset_name);

            if (asset_type == SpriteType.SpriteAtlas)
            {
                if (this.m_cacheSpriteAtlas.TryOnlyGet(image_path, out SpriteAtlasValue value))
                {
                    if (value.ref_count > 0)
                    {
                        var subasset = value.subasset;
                        if (subasset.ContainsKey(subasset_name))
                        {
                            subasset[subasset_name].ref_count = subasset[subasset_name].ref_count - 1;
                            if (subasset[subasset_name].ref_count <= 0)
                            {
                                GameObject.Destroy(subasset[subasset_name].asset);
                                subasset.Remove(subasset_name);
                            }
                            value.ref_count = value.ref_count - 1;
                        }
                    }
                }
            }
            else if (asset_type == SpriteType.DynSpriteAtlas)
            {
                var index = asset_address.IndexOf(this.DYN_ATLAS_KEY);
                var path = asset_address.Substring(0, index);
                if (this.m_cacheDynamicAtlas.TryGetValue(path, out var value))
                {
                    value.RemoveTexture(image_path, true);
                }
            }
            else
            {
                if (this.m_cacheSingleSprite.TryOnlyGet(image_path, out SpriteValue value))
                {
                    if (value.ref_count > 0)
                    {
                        value.ref_count = value.ref_count - 1;
                    }
                }
            }

        }


        /// <summary>
        /// 异步加载图集： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        /// </summary>
        /// <param name="atlas_path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadAtlasImageAsync(string atlas_path, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, atlas_path.GetHashCode());
                res = await this.__LoadAtlasImageAsyncInternal(atlas_path, null, callback);
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
        /// <param name="atlas_path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async ETTask<Sprite> LoadSingleImageAsync(string atlas_path, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockManager.Instance.Wait(CoroutineLockType.Resources, atlas_path.GetHashCode());
                res = await this.__LoadSingleImageAsyncInternal(atlas_path, callback);
                callback?.Invoke(res);

            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        #region private

        async ETTask<Sprite> __LoadAtlasImageAsyncInternal(string asset_address, string subasset_name, Action<Sprite> callback = null)
        {
            var cacheCls = this.m_cacheSpriteAtlas;
            if (cacheCls.TryGet(asset_address, out var value_c))
            {
                if (value_c.asset == null)
                {
                    cacheCls.Remove(asset_address);
                }
                else
                {
                    value_c.ref_count = value_c.ref_count + 1;
                    if (value_c.subasset.TryGetValue(subasset_name, out var result))
                    {
                        value_c.subasset[subasset_name].ref_count = value_c.subasset[subasset_name].ref_count + 1;
                        callback?.Invoke(result.asset);
                        return result.asset;
                    }
                    else
                    {
                        var sp = value_c.asset.GetSprite(subasset_name);
                        if (sp == null)
                        {
                            Log.Error("image not found:" + subasset_name);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.subasset == null)
                            value_c.subasset = new Dictionary<string, SpriteValue>();
                        value_c.subasset[subasset_name] = new SpriteValue { asset = sp, ref_count = 1 };
                        callback?.Invoke(sp);
                        return sp;
                    }
                }
            }
            var asset = await ResourcesManager.Instance.LoadAsync<SpriteAtlas>(asset_address);
            if (asset != null)
            {
                if (cacheCls.TryGet(asset_address, out var value))
                {
                    value.ref_count = value.ref_count + 1;
                }
                else
                {
                    value = new SpriteAtlasValue() { asset = asset , ref_count = 1 };
                    cacheCls.Set(asset_address, value);
                }
                if (value.subasset.TryGetValue(subasset_name, out var result))
                {
                    value.subasset[subasset_name].ref_count = value.subasset[subasset_name].ref_count + 1;
                    callback?.Invoke(result.asset);
                    return result.asset;
                }
                else
                {
                    var sp = value.asset.GetSprite(subasset_name);
                    if (sp == null)
                    {
                        Log.Error("image not found:" + subasset_name);
                        callback?.Invoke(null);
                        return null;
                    }
                    if (value.subasset == null)
                        value.subasset = new Dictionary<string, SpriteValue>();
                    value.subasset[subasset_name] = new SpriteValue { asset = sp, ref_count = 1 };
                    callback?.Invoke(sp);
                    return sp;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        async ETTask<Sprite> __LoadSingleImageAsyncInternal(string asset_address, Action<Sprite> callback = null)
        {
            var cacheCls = this.m_cacheSingleSprite;
            if (cacheCls.TryGet(asset_address, out var value_c))
            {
                if (value_c.asset == null)
                {
                    cacheCls.Remove(asset_address);
                }
                else
                {
                    value_c.ref_count = value_c.ref_count + 1;
                    callback?.Invoke(value_c.asset);
                    return value_c.asset;
                }
            }
            var asset = await ResourcesManager.Instance.LoadAsync<Sprite>(asset_address);
            if (asset != null)
            {
                if (cacheCls.TryGet(asset_address, out var value))
                {
                    value.ref_count = value.ref_count + 1;
                }
                else
                {
                    value = new SpriteValue() { asset = asset, ref_count = 1 };
                    cacheCls.Set(asset_address, value);
                    callback?.Invoke(value.asset);
                    return value.asset;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        void __GetSpriteLoadInfoByPath(string image_path, out int asset_type, out string asset_address, out string subasset_name)
        {
            asset_address = image_path;
            subasset_name = "";
            var index = image_path.IndexOf(this.ATLAS_KEY);
            if (index < 0)
            {
                //没有找到/atlas/，则是散图
                index = image_path.IndexOf(this.DYN_ATLAS_KEY);
                if (index < 0)
                {
                    //是散图
                    asset_type = SpriteType.Sprite;
                    return;
                }
                else
                {
                    //是动态图集
                    asset_type = SpriteType.DynSpriteAtlas;
                    return;
                }
            }
            asset_type = SpriteType.SpriteAtlas;
            var substr = image_path.Substring(index + this.ATLAS_KEY.Length);
            var subIndex = substr.IndexOf('/');
            string atlasPath;
            string spriteName;
            if (subIndex >= 0)
            {
                //有子目录
                var prefix = image_path.Substring(0, index+1);
                var name = substr.Substring(0, subIndex);
                atlasPath = string.Format("{0}{1}.spriteatlas", prefix, "Atlas_" + name);
                var dotIndex = substr.LastIndexOf(".");
                var lastSlashIndex = substr.LastIndexOf('/');
                spriteName = substr.Substring(lastSlashIndex+1, dotIndex - lastSlashIndex-1);
            }
            else
            {
                var prefix = image_path.Substring(0, index + 1);

                atlasPath = prefix + "Atlas.spriteatlas";


                var dotIndex = substr.LastIndexOf(".");

                spriteName = substr.Substring(0, dotIndex);
            }
            asset_address = atlasPath;
            subasset_name = spriteName;
        }

        async ETTask<Sprite> __LoadSpriteImageAsyncInternal(
             string asset_address, string subasset_name, Action<Sprite> callback)
        {
            LruCache<string, SpriteAtlasValue> cacheCls = this.m_cacheSpriteAtlas;
            var cached = false;
            if (cacheCls.TryGet(asset_address, out SpriteAtlasValue value_c))
            {
                if (value_c.asset == null)
                {
                    cacheCls.Remove(asset_address);
                }
                else
                {
                    cached = true;
                    Sprite result;
                    var subasset_list = value_c.subasset;
                    if (subasset_list.ContainsKey(subasset_name))
                    {
                        result = subasset_list[subasset_name].asset;
                        subasset_list[subasset_name].ref_count = subasset_list[subasset_name].ref_count + 1;
                        value_c.ref_count++;
                    }
                    else
                    {
                        result = value_c.asset.GetSprite(subasset_name);
                        if (result == null)
                        {
                            Log.Error("image not found:" + asset_address + "__" + subasset_name);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.subasset == null)
                            value_c.subasset = new Dictionary<string, SpriteValue>();
                        value_c.subasset[subasset_name] = new SpriteValue { asset = result, ref_count = 1 };
                        value_c.ref_count++;
                    }
                    callback?.Invoke(result);
                    return result;
                }
            }
            if (!cached)
            {
                var asset = await ResourcesManager.Instance.LoadAsync<SpriteAtlas>(asset_address);
                if (asset != null)
                {
                    Sprite result;
                    var sa = asset;
                    if (cacheCls.TryGet(asset_address, out value_c))
                    {
                        var subasset_list = value_c.subasset;
                        if (subasset_list.ContainsKey(subasset_name))
                        {
                            result = subasset_list[subasset_name].asset;
                            subasset_list[subasset_name].ref_count = subasset_list[subasset_name].ref_count + 1;
                        }
                        else
                        {
                            result = value_c.asset.GetSprite(subasset_name);
                            if (result == null)
                            {
                                Log.Error("image not found:" + asset_address + "__" + subasset_name);
                                callback?.Invoke(null);
                                return null;
                            }
                            if (value_c.subasset == null)
                                value_c.subasset = new Dictionary<string, SpriteValue>();
                            value_c.subasset[subasset_name] = new SpriteValue { asset = result, ref_count = 1 };
                        }
                    }
                    else
                    {
                        value_c = new SpriteAtlasValue { asset = sa, subasset = new Dictionary<string, SpriteValue>(), ref_count = 1 };
                        result = value_c.asset.GetSprite(subasset_name);
                        if (result == null)
                        {
                            Log.Error("image not found:" + asset_address + "__" + subasset_name);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.subasset == null)
                            value_c.subasset = new Dictionary<string, SpriteValue>();
                        value_c.subasset[subasset_name] = new SpriteValue { asset = result, ref_count = 1 };
                        cacheCls.Set(asset_address, value_c);
                    }
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    callback?.Invoke(null);
                    return null;
                }

            }
            callback?.Invoke(null);
            return null;

        }

        async ETTask<Sprite> __LoadDynSpriteImageAsyncInternal(
             string asset_address, Action<Sprite> callback)
        {
            Dictionary<string, DynamicAtlas> cacheCls = this.m_cacheDynamicAtlas;
            var index = asset_address.IndexOf(this.DYN_ATLAS_KEY);
            var path = asset_address.Substring(0, index);
            if (cacheCls.TryGetValue(path, out DynamicAtlas value_c))
            {
                Sprite result;
                if (value_c.TryGetSprite(asset_address,out result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    var texture = await ResourcesManager.Instance.LoadAsync<Texture>(asset_address);
                    if (texture == null)
                    {
                        Log.Error("image not found:" + asset_address);
                        callback?.Invoke(null);
                        return null;
                    }
                    value_c.SetTexture(asset_address,texture);
                    ResourcesManager.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                    if (value_c.TryGetSprite(asset_address,out  result))
                    {
                        callback?.Invoke(result);
                        return result;
                    }
                    Log.Error("image not found:" + asset_address );
                    callback?.Invoke(null);
                    return null;
                }
            }
            else
            {
                // Log.Info(this.Id +" "+ cacheCls.Count);
                Log.Info("CreateNewDynamicAtlas  ||"+path+"||");
                value_c = new DynamicAtlas(DynamicAtlasGroup.Size_2048);
                cacheCls.Add(path,value_c);
                // Log.Info(this.Id +" "+ cacheCls.Count);
                var texture = await ResourcesManager.Instance.LoadAsync<Texture>(asset_address);
                if (texture == null)
                {
                    Log.Error("image not found:" + asset_address );
                    callback?.Invoke(null);
                    return null;
                }
                value_c.SetTexture(asset_address,texture);
                ResourcesManager.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                if (value_c.TryGetSprite(asset_address,out var result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                Log.Error("image not found:" + asset_address);
                callback?.Invoke(null);
                return null;
            }
        }
        #endregion

        public void Clear()
        {
            foreach ((string key,var value) in this.m_cacheSpriteAtlas)
            {
                if (value.subasset != null)
                    foreach (var item in value.subasset)
                    {
                        GameObject.Destroy(item.Value.asset);
                    }
                ResourcesManager.Instance?.ReleaseAsset(value.asset);
                value.asset = null;
                value.subasset = null;
                value.ref_count = 0;
            }
            this.m_cacheSpriteAtlas.Clear();

            foreach ((string key, var value) in this.m_cacheSingleSprite)
            {
                ResourcesManager.Instance?.ReleaseAsset(value.asset);
                value.asset = null;
                value.ref_count = 0;
            }
            this.m_cacheSingleSprite.Clear();

            foreach (var item in this.m_cacheDynamicAtlas)
            {
                item.Value.Dispose();
            }
            this.m_cacheDynamicAtlas.Clear();
            Log.Info("ImageLoaderManager Clear");
        }
    }
}