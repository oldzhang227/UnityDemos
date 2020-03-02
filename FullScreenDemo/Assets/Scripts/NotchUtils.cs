using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotchUtils: MonoBehaviour
{
    public bool HasNotch = false;
    public float NotchScale = 0.0f;
    public float NotchSize = 0;
    public bool IsInit = false;
    public static NotchUtils _instance = null;
    public Rect SafeRect = new Rect(0, 0, 1, 1);
    public static NotchUtils Instance()
    {
        if(_instance == null)
        {
            GameObject obj = new GameObject("NotchUtils");
            _instance = obj.AddComponent<NotchUtils>();
            DontDestroyOnLoad(obj);
        }
        return _instance;
    }

    private void Awake()
    {
        StartCoroutine(InitNotch());
    }

    private IEnumerator InitNotch()
    {
#if UNITY_EDITOR
        NotchScale = 0.2f;
        HasNotch = true;
#elif UNITY_ANDROID
        yield return StartCoroutine(InitNotchAndroid());
#elif UNITY_IOS
        yield return StartCoroutine(InitNotchiOS());
#endif
        IsInit = true;
        yield return null;
    }

    public Rect GetSafeRect()
    {
        Rect safeRect = new Rect(0, 0, 1, 1);
        if(HasNotch)
        {
            if(Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                safeRect.x = NotchScale;
                safeRect.width = 1 - NotchScale;
            }
            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                safeRect.x = 0;
                safeRect.width = 1 - NotchScale;
            }
        }
        return safeRect;
    }

    private IEnumerator InitNotchAndroid()
    {
        AndroidJavaClass notchUtilsClass = new AndroidJavaClass("com.utils.notch.NotchUtils");
        if(notchUtilsClass != null)
        {
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            notchUtilsClass.CallStatic("init", new object[] { currentActivity });
            bool isInitDone = false;
            while(!isInitDone)
            {
                isInitDone = notchUtilsClass.CallStatic<bool>("isInitDone");
                yield return null;
            }
            HasNotch = notchUtilsClass.CallStatic<bool>("hasNotch");
            if(HasNotch)
            {
                NotchSize = notchUtilsClass.CallStatic<int>("getNotchHeight");
                if(NotchSize > 0)
                {
                    NotchScale = NotchSize / Screen.width;
                }
                else
                {
                    HasNotch = false;
                }
            }
        }
        yield return null;
    }

    private IEnumerator InitNotchiOS()
    {
        float notchSize = Screen.safeArea.y;
        if (notchSize > 0.0f)
        {
            NotchSize = notchSize;
            HasNotch = true;
            NotchScale = NotchSize / Screen.width;
        }
        yield return null;
    }
}
