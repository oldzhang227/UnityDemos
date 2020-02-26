using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 缓存池管理器
/// </summary>
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    class ObjectInfo
    {
        public string assetName;
        public System.WeakReference　weakReference;
    }


    private Dictionary<int, ObjectInfo> _objInfoDict;
    private Dictionary<string, ObjectPool> _poolDict;



    protected override void Init()
    {
        _objInfoDict = new Dictionary<int, ObjectInfo>();
        _poolDict = new Dictionary<string, ObjectPool>();
    }


    public void Update()
    {
        PoolUpdate();
        RemoveUnusedInfo();
    }


    /// <summary>
    /// 清空缓存池
    /// </summary>
    public void ClearPool()
    {
        foreach (var p in _poolDict)
        {
            p.Value.Clear();
        }
        _poolDict.Clear();
    }

    /// <summary>
    /// 加入到缓存池中
    /// </summary>
    /// <param name="obj"></param>
    public void InPool(GameObject obj)
    {
        int instanceId = obj.GetInstanceID();
        if (_objInfoDict.ContainsKey(instanceId))
        {
            string assetName = _objInfoDict[instanceId].assetName;
            ObjectPool objectPool = null;
            if (!_poolDict.TryGetValue(assetName, out objectPool))
            {
                objectPool = new ObjectPool();
                objectPool.name = assetName;
                _poolDict.Add(assetName, objectPool);
            }
            objectPool.InPool(obj);
        }
    }


    /// <summary>
    /// 从缓存池中获取对象
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public GameObject GetObject(string assetName)
    {
        ObjectPool objectPool = null;
        if (_poolDict.TryGetValue(assetName, out objectPool))
        {
            return objectPool.Get();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 实例化后存储GameObject对应的Asset名字，方便回收
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="obj"></param>
    public void StoreObject(string assetName, GameObject obj)
    {
        int instanceId = obj.GetInstanceID();
        if (!_objInfoDict.ContainsKey(instanceId))
        {
            ObjectInfo objectInfo = new ObjectInfo();
            objectInfo.assetName = assetName;
            objectInfo.weakReference = new System.WeakReference(obj);
            _objInfoDict.Add(instanceId, objectInfo);
        }
    }

    /// <summary>
    /// 销毁前删除Object信息
    /// </summary>
    /// <param name="obj"></param>
    public void Destroy(GameObject obj)
    {
        if(obj != null)
        {
            int instanceId = obj.GetInstanceID();
            if (_objInfoDict.ContainsKey(instanceId))
            {
                _objInfoDict.Remove(instanceId);
            }
            GameObject.Destroy(obj);
        }
    }


    /// <summary>
    /// 缓存池刷新，清空
    /// </summary>
    private void PoolUpdate()
    {
        foreach (var p in _poolDict)
        {
            p.Value.Update();
        }
    }

    /// <summary>
    /// 删除无效的GameObjec信息
    /// </summary>
    private void RemoveUnusedInfo()
    {
        List<int> removeList = new List<int>();
        foreach(var p in _objInfoDict)
        {
            if(!p.Value.weakReference.IsAlive)
            {
                removeList.Add(p.Key);
            }  
        }
        for(int i = 0; i < removeList.Count; ++i)
        {
            _objInfoDict.Remove(removeList[i]);
        }
    }

}
