using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> where T: Singleton<T>, new()
{
    private static T s_instance;

    public static T Instance
    {
        get
        {
            if(s_instance == null)
            {
                s_instance = new T();
                s_instance.Init();
            }
            return s_instance;
        }
    }

    protected virtual void Init() { }

    public virtual void Release() { }

    public static void Destory()
    {
        s_instance.Release();
        s_instance = default(T);
    }
}