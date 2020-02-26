using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// AssetBundle
/// </summary>
public class AssetBundleInfo
{
    public string name;
    public AssetBundle assetBundle { get { return _assetBundle; } }
    // 加载的AssetBundle
    private AssetBundle _assetBundle = null;
    // 依赖的AssetBundle
    private List<AssetBundleInfo> _dependAssetBundles = null;
    // 被引用的AssetBundle
    private List<AssetBundleInfo> _refAssetBundles = null;
    // 被引用的Asset
    private List<System.WeakReference> _refAssets = null;
    // 异步加载的AssetBundle
    private AssetBundleCreateRequest _assetBundleCreateRequest = null;
    // 异步加载的任务
    private Dictionary<string, AssetLoader> _assetLoaders = null;

    #region AssetBundle
    /// <summary>
    /// 添加依赖的AssetBundle
    /// </summary>
    /// <param name="assetBundleInfo"></param>
    public void AddDependAssetBundle(AssetBundleInfo assetBundleInfo)
    {
        if(_dependAssetBundles == null)
        {
            _dependAssetBundles = new List<AssetBundleInfo>();
        }
        _dependAssetBundles.Add(assetBundleInfo);
    }

    /// <summary>
    /// 添加被引用的AssetBundle
    /// </summary>
    /// <param name="assetBundleInfo"></param>
    public void AddRefAssetBundle(AssetBundleInfo assetBundleInfo)
    {
        if (_refAssetBundles == null)
        {
            _refAssetBundles = new List<AssetBundleInfo>();
        }
        _refAssetBundles.Add(assetBundleInfo);
    }

    /// <summary>
    /// 获取依赖的AssetBundle
    /// </summary>
    /// <returns></returns>
    public List<AssetBundleInfo> GetDependAssetBundeInfos()
    {
        return _dependAssetBundles;
    }

    /// <summary>
    /// 检查异步加载是否完成
    /// </summary>
    /// <returns></returns>
    public bool CheckLoading()
    {
        if (_assetBundleCreateRequest != null)
        {
            if (_assetBundleCreateRequest.isDone)
            {
                _assetBundle = _assetBundleCreateRequest.assetBundle;
                _assetBundleCreateRequest = null;
            }
            else
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 是否正在加载AssetBundle
    /// </summary>
    /// <returns></returns>
    public bool IsLoading()
    {
        return _assetBundleCreateRequest != null;
    }

    /// <summary>
    /// 是否正在加载AssetBundle,判断依赖
    /// </summary>
    /// <returns></returns>
    public bool IsLoadingByDepends()
    {
        if (IsLoading())
        {
            return true;
        }
        if(_dependAssetBundles != null)
        {
            for (int i = 0, count = _dependAssetBundles.Count; i < count; ++i)
            {
                if (_dependAssetBundles[i].IsLoading())
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 同步加载AssetBundle
    /// </summary>
    public void LoadAssetBundle()
    {
        if (_assetBundle == null)
        {
            _assetBundle = AssetBundle.LoadFromFile(GetAssetBundleFilePath(name));
            _assetBundleCreateRequest = null;
        }
    }

    /// <summary>
    /// 异步加载AssetBundle
    /// </summary>
    public void LoadAssetBundleAsync()
    {
        if (_assetBundle == null && _assetBundleCreateRequest == null)
        {
            _assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(GetAssetBundleFilePath(name));
        }
    }

    /// <summary>
    /// 判断没有引用时卸载
    /// </summary>
    public void UnloadUnused(bool unloadAllLoadedObjects)
    {
        if (_assetBundle == null)
        {
            return;
        }
        if(IsUnused() == true)
        {
            _assetBundle.Unload(unloadAllLoadedObjects);
            _assetBundle = null;
            Debug.LogFormat("UnloadUnused AssetBundle:{0}", name);
        }
    }

    /// <summary>
    /// 强制卸载
    /// </summary>
    public void ForceUnload(bool unloadAllLoadedObjects)
    {
        if (_assetBundle != null)
        {
            _assetBundle.Unload(unloadAllLoadedObjects);
            _assetBundle = null;
        }
    }

    /// <summary>
    /// 获取当前的平台名
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return "windows";
#elif UNITY_ANDROID
        return "android";
#elif UNITY_IOS
        return "ios";
#endif
    }
    /// <summary>
    /// AssetBundle文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetAssetBundleFilePath(string assetBundleName)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(Application.streamingAssetsPath);
        sb.Append("/assetbundles/");
        sb.Append(GetPlatformName());
        sb.Append("/");
        sb.Append(assetBundleName);
        return sb.ToString();
    }
    #endregion

    #region Asset
    /// <summary>
    /// 添加Asset引用
    /// </summary>
    /// <param name="asset"></param>
    public void AddAssetRef(Object asset)
    {
        if (asset != null && !IsRefAsset(asset))
        {
            if (_refAssets == null)
            {
                _refAssets = new List<System.WeakReference>();
            }
            _refAssets.Add(new System.WeakReference(asset));
        }
    }

    /// <summary>
    /// 是否引用Asset
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public bool IsRefAsset(Object asset)
    {
        if (_refAssets == null)
        {
            return false;
        }
        for (int i = 0, count = _refAssets.Count; i < count; i++)
        {
            if (_refAssets[i].Target == (object)asset)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查是否引用Asset
    /// </summary>
    /// <returns></returns>
    public bool CheckAssetRef()
    {
        if (_refAssets == null)
        {
            return false;
        }
        for (int i = _refAssets.Count - 1; i >= 0; i--)
        {
            if ((UnityEngine.Object)(_refAssets[i].Target) == null)
            {
                _refAssets.RemoveAt(i);
            }
        }
        if (_refAssets.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 是否未使用，可卸载的
    /// </summary>
    /// <returns></returns>
    public bool IsUnused()
    {
        if(IsByAssetLoaderRef())
        {
            return false;
        }
        if(_refAssetBundles != null)
        {
            for(int i = 0, count = _refAssetBundles.Count; i < count; ++i)
            {
                if(_refAssetBundles[i].CheckAssetRef())
                {
                    return false;
                }
            }
        }
        return !CheckAssetRef();
    }
    #endregion

    #region AssetLoader
    /// <summary>
    /// 是否正在被加载任务引用
    /// </summary>
    /// <returns></returns>
    public bool IsByAssetLoaderRef()
    {
        if (_assetLoaders != null && _assetLoaders.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 添加加载任务的引用
    /// </summary>
    /// <param name="loader"></param>
    public void AddRefAssetLoader(AssetLoader loader)
    {
        if (_assetLoaders == null)
        {
            _assetLoaders = new Dictionary<string, AssetLoader>();
        }
        if (!_assetLoaders.ContainsKey(loader.assetName))
        {
            _assetLoaders.Add(loader.assetName, loader);
        }
    }

    /// <summary>
    /// 删除加载任务的引用
    /// </summary>
    /// <param name="loader"></param>
    public void RemoveRefAssetLoader(AssetLoader loader)
    {
        if (_assetLoaders.ContainsKey(loader.assetName))
        {
            _assetLoaders.Remove(loader.assetName);
        }
    }
    #endregion
}
