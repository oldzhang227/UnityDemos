using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 缓存池
/// </summary>
public class ObjectPool
{
    public string name;
    public const int maxCount = 5;
    public const float liveTime = 2.0f;
    private Stack<GameObject> _objList = new Stack<GameObject>();
    private float _lastVisitTime = 0.0f;
    private static GameObject s_pool = null;

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <returns></returns>
    public GameObject Get()
    {
        _lastVisitTime = Time.realtimeSinceStartup;
        GameObject obj = null;
        if (_objList != null && _objList.Count > 0)
        {
            obj = _objList.Pop();
            obj.transform.SetParent(null);
            obj.SetActive(true);
        }
        return obj;
    }


    /// <summary>
    /// 加入缓存池，有数量限制
    /// </summary>
    /// <param name="obj"></param>
    public void InPool(GameObject obj)
    {
        _lastVisitTime = Time.realtimeSinceStartup;
        if (_objList.Count >= maxCount)
        {
            ObjectPoolManager.Instance.Destroy(obj);
            return;
        }
        if (s_pool == null)
        {
            s_pool = new GameObject("pool");
            GameObject.DontDestroyOnLoad(s_pool);
        }
        obj.transform.SetParent(s_pool.transform);
        obj.SetActive(false);
        _objList.Push(obj);
    }

    /// <summary>
    /// 按照时间依次销毁
    /// </summary>
    public void Update()
    {
        if (_objList.Count == 0)
        {
            return;
        }
        float t = Time.realtimeSinceStartup - _lastVisitTime;
        while (_objList.Count > 0 && t > (maxCount - _objList.Count + 1) * liveTime)
        {
            ObjectPoolManager.Instance.Destroy(_objList.Pop());
        }
    }


    /// <summary>
    /// 是否为空
    /// </summary>
    /// <returns></returns>
    public bool Empty()
    {
        return _objList.Count == 0;
    }


    /// <summary>
    /// 清空缓存
    /// </summary>
    public void Clear()
    {
        while(_objList.Count > 0)
        {
            ObjectPoolManager.Instance.Destroy(_objList.Pop());
        }
    }
}
