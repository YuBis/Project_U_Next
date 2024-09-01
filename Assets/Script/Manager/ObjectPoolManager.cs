using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Collections;
using System;
using Cysharp.Threading.Tasks;

public struct stPoolData
{
    public GameObject POOL_OBJECT;
    public long LAST_USE_TICKS;
}

public class ObjectPoolManager : BaseManager<ObjectPoolManager>
{
    Dictionary<string, (int BaseCapa, List<stPoolData> PoolList)> m_dicPoolData = new();

    const int RELEASE_MINUTES = 5;

    protected override void _InitManager()
    {
        _UpdatePoolData().Forget();
    }

    async UniTaskVoid _UpdatePoolData()
    {
        List<string> listDeleteKey = new();

        var nextRemoveTicks = DateTime.Now.Ticks + TimeSpan.TicksPerMinute;

        while (true)
        {
            var currentTicks = DateTime.Now.Ticks;
            if (nextRemoveTicks < currentTicks)
            {
                nextRemoveTicks = currentTicks + TimeSpan.TicksPerMinute;
                foreach (var pair in m_dicPoolData)
                {
                    var poolDataList = pair.Value.PoolList;
                    while (poolDataList.Count > pair.Value.BaseCapa && poolDataList.Count > 0)
                    {
                        if (poolDataList[0].LAST_USE_TICKS + (TimeSpan.TicksPerMinute * RELEASE_MINUTES) < currentTicks)
                        {
                            GameObject.Destroy(poolDataList[0].POOL_OBJECT);
                            poolDataList.RemoveAt(0);
                        }
                        else
                            break;
                    }

                    if (poolDataList.Count == 0)
                        listDeleteKey.Add(pair.Key);
                }

                foreach(var delKey in listDeleteKey)
                {
                    m_dicPoolData.Remove(delKey);
                }

                listDeleteKey.Clear();
            }

            await UniTask.NextFrame();
        }
    }

    public bool HasPoolData(GameObject prefabObject)
    {
        if (!prefabObject)
            return false;

        return m_dicPoolData.ContainsKey(prefabObject.name) &&
            m_dicPoolData[prefabObject.name].PoolList.Count > 0;
    }

    public void AddPoolData(GameObject prefabObject, int capa = 0)
    {
        if (prefabObject == null || capa < 0)
            return;

        if (m_dicPoolData.ContainsKey(prefabObject.name))
            return;

        m_dicPoolData[prefabObject.name] = (capa, new List<stPoolData>());

        var prefabPoolData = new stPoolData()
        {
            POOL_OBJECT = prefabObject,
            LAST_USE_TICKS = DateTime.Now.Ticks
        };

        for (int i = 0; i < capa; i++)
        {
            m_dicPoolData[prefabObject.name].PoolList.Add(prefabPoolData);
        }
        //m_dicPoolData[prefabObject.name].PoolList.AddRange(Enumerable.Repeat(prefabPoolData, capa));
    }

    public GameObject GetObject(GameObject prefabObject, bool bActive = true, int capa = 0)
    {
        if (prefabObject == null)
            return null;

        if (!HasPoolData(prefabObject) && capa >= 1)
            AddPoolData(prefabObject, capa);

        var poolObj = _GetObjectFromPool(prefabObject.name, bActive);
        if (poolObj)
            return poolObj;

        GameObject makeObject = GameObject.Instantiate<GameObject>(prefabObject);
        if (makeObject)
            makeObject.SetActive(bActive);

        return makeObject;
    }

    public void GetObject(string prefabName, Action<GameObject> callBack, bool bActive = true)
        => _GetObject(prefabName, callBack, bActive).Forget();

    async UniTaskVoid _GetObject(string prefabName, Action<GameObject> callBack, bool bActive = true)
    {
        var poolObj = _GetObjectFromPool(prefabName, bActive);
        if (poolObj)
        {
            callBack?.Invoke(poolObj);
            return;
        }

        await _MakeNewPrefabObject(prefabName, callBack, bActive);
    }

    GameObject _GetObjectFromPool(string prefabName, bool bActive = true)
    {
        if (m_dicPoolData.TryGetValue(prefabName, out var poolData) && poolData.PoolList.Count > 0)
        {
            var poolList = poolData.PoolList;
            int lastIdx = poolList.Count - 1;

            var retObject = poolList[lastIdx];
            poolList.RemoveAt(lastIdx);

            if (retObject.POOL_OBJECT)
            {
                retObject.POOL_OBJECT.SetActive(bActive);
                return retObject.POOL_OBJECT;
            }
        }

        return null;
    }

    async UniTask _MakeNewPrefabObject(string prefabName, Action<GameObject> callBack, bool bActive)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(prefabName);
        await op;

        var prefabObject = op.Result;
        if (!prefabObject)
        {
            Universe.LogError($"Cannot load {prefabName}!");
            return;
        }

        var makeObject = GameObject.Instantiate<GameObject>(prefabObject);

        makeObject.name = makeObject.name.Replace("(Clone)", "");
        makeObject.SetActive(bActive);

        callBack?.Invoke(makeObject);
    }

    public void ReleaseObject(GameObject prefabObject)
    {
        if (prefabObject == null)
            return;

        string prefabName = prefabObject.name.Replace("(Clone)", "");

        if (!m_dicPoolData.TryGetValue(prefabName, out var poolData))
            poolData = (0, new List<stPoolData>());


        prefabObject.transform.SetParent(Universe.GetCarrier().TRANSFORM, false);
        prefabObject.SetActive(false);
        poolData.PoolList.Add(new stPoolData
        {
            POOL_OBJECT = prefabObject,
            LAST_USE_TICKS = DateTime.Now.Ticks
        });

        m_dicPoolData[prefabName] = poolData;
    }
}