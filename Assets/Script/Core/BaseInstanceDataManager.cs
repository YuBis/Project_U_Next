using SimpleJSON;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BaseInstanceDataManager<MANAGER_TYPE, DATA_TYPE>
    where MANAGER_TYPE : class, new()
    where DATA_TYPE : BaseInstanceData, new()
{
    protected Dictionary<string, DATA_TYPE> m_dicData = new Dictionary<string, DATA_TYPE>();

    static MANAGER_TYPE s_instance = null;
    public static MANAGER_TYPE Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new MANAGER_TYPE();
            }

            return s_instance;
        }
    }

    virtual public DATA_TYPE CreateInstanceData() => new DATA_TYPE();

    public DATA_TYPE GetData(string key)
    {
        if (m_dicData.TryGetValue(key, out var data))
            return data;

        return null;
    }
}
