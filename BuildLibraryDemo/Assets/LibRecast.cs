using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class LibRecast
{
#if UNITY_EDITOR
    public const string LibName = "libRecast";
#elif UNITY_ANDROID
    public const string LibName = "Recast";
#elif UNITY_IOS
    public const string LibName = "__Internal";
#endif

    [DllImport(LibName)]
    public extern static int TestAPI();


}
