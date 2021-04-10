
using System;
using System.Collections;
using System.Collections.Generic;


namespace IFix.Bridge
{
    [IFix.CustomBridge]
    public static class ILFixCustomBridge
    {
        static List<Type> bridge = new List<Type>()
        {
			typeof(IEnumerator),
            typeof(IDisposable),
            typeof(IEnumerator<object>),
        };
    }
}