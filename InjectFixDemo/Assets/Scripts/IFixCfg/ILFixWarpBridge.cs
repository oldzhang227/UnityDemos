
using System;
using System.Collections.Generic;


namespace IFix.Bridge
{
    [IFix.CustomBridge]
    public static class ILFixWarpBridge
    {
        static List<Type> bridge = new List<Type>()
        {
			typeof(System.Action<System.String>),
			typeof(Game.InjectDemo.MyDelegate),

        };
    }
}