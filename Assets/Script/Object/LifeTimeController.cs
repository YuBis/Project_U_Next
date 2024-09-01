using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class LifeTimeController : BaseObject
{
    [SerializeField]
    float m_lifeTime = 1;

    float m_leftTime = 0;
    private void OnEnable()
    {
        m_leftTime = m_lifeTime;
        BeginCountdown().Forget();
    }

    async UniTaskVoid BeginCountdown()
    {
        while(m_leftTime > 0)
        {
            m_leftTime -= Time.deltaTime;
            await UniTask.NextFrame();
        }

        if(GAMEOBJECT)
            GAMEOBJECT.SetActive(false);
    }
}
