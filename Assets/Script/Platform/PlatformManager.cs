using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformManager
{
    public static PlatformManager Instance = null;

#if (UNITY_ANDROID)
        public static Platform_Android   Platform { get; private set; }
#elif (UNITY_IPHONE)
        public static Platform_IOS       Platform { get; private set; }
#else
    public static Platform_Window    Platform { get; private set; }
    #endif

    public void InitManager()
    {
        #if (UNITY_ANDROID)
            Platform = Platform_Android.Instance as Platform_Android;
        #elif (UNITY_IPHONE)
            Platform = Platform_IOS.Instance as Platform_IOS;
        #else
            Platform = Platform_Window.Instance as Platform_Window;
        #endif
    }
}