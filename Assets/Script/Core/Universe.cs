using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Universe
{
    static Carrier m_carrier = null;
    static System.Random m_rand = new System.Random();

    static public Carrier GetCarrier()
    {
        if (m_carrier == null)
        {
            var carrier = new GameObject("CARRIER");
            m_carrier = carrier.AddComponent<Carrier>();
            UnityEngine.Object.DontDestroyOnLoad(carrier);
        }

        return m_carrier;
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    static public void LogError(string str) => UnityEngine.Debug.LogError(str);

    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    static public void LogWarning(string str) => UnityEngine.Debug.LogWarning(str);

    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    static public void LogDebug(string str) => UnityEngine.Debug.Log(str);

    static public double GetDoubleRandom(double min, double max)
    {
        if (min >= max)
            return min;

        return m_rand.NextDouble() * (max - min) * min;
    }

    static public int GetIntRandom(int min, int max)
    {
        if (min >= max)
            return min;

        return m_rand.Next(min, max);
    }
}
