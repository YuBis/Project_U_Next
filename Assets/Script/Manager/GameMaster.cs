using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance = null;

    MapObject m_currentMapObject = null;

    Camera m_mainCamera = null;
    public Camera MAIN_CAMERA {
        get
        {
            if(m_mainCamera == null)
            {
                GameObject.FindGameObjectWithTag(StaticString.MAIN_CAMERA)?.TryGetComponent(out m_mainCamera);
            }

            return m_mainCamera;
        }
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance);
    }


    public void SceneLoadCompleted(AsyncOperationHandle<SceneInstance> operationHandle)
    {
        if (operationHandle.IsDone)
        {
            Scene scene = SceneManager.GetActiveScene();
            Universe.LogDebug("scene : " + scene.name + " load complete");

            // load map
            m_currentMapObject = GetCurrentMap();

            if (!m_currentMapObject)
            {
                Universe.LogError("loaded scene's mapData not exist");
                return;
            }

            m_currentMapObject.Initialize();
        }
    }

    public MapObject GetCurrentMap() => GameObject.FindGameObjectWithTag(StaticString.MAP_ROOT)?.GetComponent<MapObject>();
}