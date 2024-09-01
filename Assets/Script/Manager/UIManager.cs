using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;

public class UIManager : BaseManager<UIManager>
{
    Dictionary<string, List<GameObject>> m_dicUI = new();
    Transform m_rootTransform = null;

    protected override void _InitManager()
    {
        ObjectPoolManager.Instance.GetObject("Pf_Ui_Root", (go) =>
        {
            m_rootTransform = go.GetComponent<Transform>();
            GameObject.DontDestroyOnLoad(go);
        });
    }

    public void MakeUI(string uiKey, Action<GameObject> callBack = null)
    {
        ObjectPoolManager.Instance.GetObject(uiKey, (go) =>
        {
            _CallbackMakeUI(uiKey, go, callBack);
        });
    }

    void _CallbackMakeUI(string uiKey, GameObject uiObject, Action<GameObject> finalCallback)
    {
        if (!m_dicUI.TryGetValue(uiKey, out var uiList))
            uiList = new();

        uiList.Add(uiObject);

        m_dicUI[uiKey] = uiList;

        if (m_rootTransform != null)
            uiObject.GetComponent<Transform>().SetParent(m_rootTransform, false);

        finalCallback?.Invoke(uiObject);
    }

    public void ReleaseUI(string uiKey, GameObject targetObject)
    {
        var targetList = m_dicUI[uiKey];
        if (targetList == null)
            return;

        targetList.Remove(targetObject);

        ObjectPoolManager.Instance.ReleaseObject(targetObject);
    }
}