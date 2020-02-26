using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Asset资源
/// </summary>
public class AssetInfo
{
    public string name;
    public bool isScene;
    public string resourcesName;
    public string assetBundleName;
    // Asset只读对象
    private Object _asset = null;
    // 实例化引用对象
    private List<System.WeakReference> _refObjects = null;
    // Resources异步加载Asset
    private ResourceRequest _resourceRequest = null;
    // AB异步加载Asset
    private AssetBundleRequest _assetBundleRequest = null;
    // 异步加载场景
    private AsyncOperation _sceneAsyncOperation = null;
    // 引用次数
    private int _refCount = 0;
    
    /// <summary>
    /// 加计数
    /// </summary>
    public void Retain()
    { 
        _refCount++;
    }

    /// <summary>
    /// 减计数
    /// </summary>
    public void Release()
    {
        _refCount = --_refCount < 0 ? 0 : _refCount;
    }
    /// <summary>
    /// 清除引用Asset
    /// </summary>
    public void ClearAsset()
    {
        _asset = null;
        _refCount = 0;
    }

    /// <summary>
    /// 未被引用则清除Asset
    /// </summary>
    public void UnloadUnused()
    {
        if(Exist() && IsUnused())
        {
            ClearAsset();
            Debug.LogFormat("UnloadUnused Asset:{0}", name);
        }
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <returns></returns>
    public bool Exist()
    {
        return _asset != null;
    }

    /// <summary>
    /// 检查异步加载是否完成
    /// </summary>
    /// <returns></returns>
    public bool CheckResourceLoading()
    {
        if(_resourceRequest != null)
        {
            if(_resourceRequest.isDone)
            {
                _asset = _resourceRequest.asset;
                _resourceRequest = null;
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 通过Resources加载
    /// </summary>
    /// <param name="async"></param>
    public void LoadByResources(bool async)
    {
        if(Exist() == true)
        {
            return;
        }
        string loadName = GetResourcesName();
        if (async == true)
        {
            if (_resourceRequest == null)
            {
                _resourceRequest = Resources.LoadAsync(loadName);
            }
        }
        else
        {
            _asset = Resources.Load(loadName);
            _resourceRequest = null;
        }
    }


    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="async"></param>
    public void LoadScene(bool single, bool async)
    {
        string sceneName = Path.GetFileNameWithoutExtension(name);
        LoadSceneMode sceneMode = single ? LoadSceneMode.Single : LoadSceneMode.Additive;
        if (async)
        {
            _sceneAsyncOperation =  SceneManager.LoadSceneAsync(sceneName, sceneMode);
        }
        else
        {
            SceneManager.LoadScene(sceneName, sceneMode);
        }
    }


    /// <summary>
    /// 检查场景正在加载
    /// </summary>
    /// <returns></returns>
    public bool CheckSceneLoading()
    {
        if(_sceneAsyncOperation != null)
        {
            if(!_sceneAsyncOperation.isDone)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 加载场景的进度
    /// </summary>
    /// <returns></returns>
    public float LoadingSceneProgress()
    {
        if (_sceneAsyncOperation != null)
        {
            return _sceneAsyncOperation.progress;
        }
        return 1.0f;
    }

    /// <summary>
    /// Resources异步加载的进度
    /// </summary>
    /// <returns></returns>
    public float LoadingByResourcesProgress()
    {
        if (_resourceRequest != null)
        {
            return _resourceRequest.progress;
        }
        return 1.0f;
    }

    /// <summary>
    /// AssetBundle异步加载的进度
    /// </summary>
    /// <returns></returns>
    public float LoadingByAssetBundleProgress()
    {
        if (_assetBundleRequest != null)
        {
            return _assetBundleRequest.progress;
        }
        return 1.0f;
    }

    /// <summary>
    /// 检查异步加载是否完成
    /// </summary>
    /// <returns></returns>
    public bool CheckAssetBundleLoading()
    {
        if (_assetBundleRequest != null)
        {
            if (_assetBundleRequest.isDone)
            {
                _asset = _assetBundleRequest.asset;
                _assetBundleRequest = null;
                return false;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 通过AssetBundle加载
    /// </summary>
    /// <param name="assetBundle"></param>
    /// <param name="async"></param>
    public void LoadByAssetBundle(AssetBundle assetBundle, bool async)
    {
        if (Exist() == true)
        {
            return;
        }
        if (async == true)
        {
            if (_assetBundleRequest == null)
            {
                _assetBundleRequest = assetBundle.LoadAssetAsync(name);
            }
        }
        else
        {
            _asset = assetBundle.LoadAsset(name);
            _assetBundleRequest = null;
        }
    }

    /// <summary>
    /// 添加引用到AssetBundle
    /// </summary>
    /// <param name="assetBundleInfo"></param>
    public void AddAssetBundleRef(AssetBundleInfo assetBundleInfo)
    {
        if (assetBundleInfo == null)
        {
            return;
        }
        if (isScene == true)
        {
            GameObject sceneObj = new GameObject(name);
            assetBundleInfo.AddAssetRef(sceneObj);
        }
        else
        {
            if (_asset != null)
            {
                assetBundleInfo.AddAssetRef(_asset);
            }
        }
    }


    /// <summary>
    /// 实例化，并记录引用
    /// </summary>
    /// <returns></returns>
    public GameObject Instantiate()
    {
        if(_asset != null)
        {
            GameObject obj = ObjectPoolManager.Instance.GetObject(name);
            if(obj == null)
            {
                obj = GameObject.Instantiate(_asset as GameObject);
                if (obj != null)
                {
                    obj.StoreUsePool(name);
                    AddRefObject(obj);
                }
            }
            return obj;
        }
        return null;
    }

    /// <summary>
    /// 实例化并记录引用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Instantiate<T>() where T : Object
    {
        if (_asset != null)
        {
            T obj = GameObject.Instantiate<T>(_asset as T);
            if (obj != null)
            {
                AddRefObject(obj);
            }
            return obj;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 记录引用
    /// </summary>
    /// <param name="obj"></param>
    public void AddRefObject(Object obj)
    {
        if(_refObjects == null)
        {
            _refObjects = new List<System.WeakReference>();
        }
        _refObjects.Add(new System.WeakReference(obj));
    }

    /// <summary>
    /// 检查实例化引用
    /// </summary>
    /// <returns></returns>
    public bool CheckObjectRef()
    {
        if (_refObjects == null)
        {
            return false;
        }
        for (int i = _refObjects.Count - 1; i >= 0; i--)
        {
            // UnityObject重载了=的判断，即使野指针了，也会判断=null
            if ((UnityEngine.Object)(_refObjects[i].Target) == null)
            {
                _refObjects.RemoveAt(i);
            }
        }
        if (_refObjects.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 是否未被使用
    /// </summary>
    /// <returns></returns>
    public bool IsUnused()
    {
        if(_refCount == 0 && !CheckObjectRef())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 获取Resources加载名字
    /// </summary>
    /// <returns></returns>
    public string GetResourcesName()
    {
        if(!string.IsNullOrEmpty(resourcesName))
        {
            return resourcesName;
        }
        int startIndex = name.IndexOf("Resources");
        if (startIndex != -1)
        {
            startIndex += 10;
            int lastIndex = name.LastIndexOf(".");
            if (lastIndex != -1)
            {
                resourcesName = name.Substring(startIndex, lastIndex - startIndex);
            }
            else
            {
                resourcesName = name.Substring(startIndex);
            }
        }
        return resourcesName;
    }

    /// <summary>
    /// 获取AssetBundle名字
    /// </summary>
    /// <returns></returns>
    public string GetAssetBundleName()
    {
        if (!string.IsNullOrEmpty(assetBundleName))
        {
            return assetBundleName;
        }
        string resName = GetResourcesName();
        if (resName != null)
        {
            if(isScene)
            {
                assetBundleName = resName + ".ab";
            }
            else
            {
                assetBundleName = System.IO.Path.GetDirectoryName(resName) + ".ab";
            }       
        }
        return assetBundleName;
    }
}
