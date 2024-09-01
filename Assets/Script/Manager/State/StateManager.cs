using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public enum StateType
{ 
    NONE,
    INIT,
    LOBBY,
    TEST
}
public class StateManager : MonoBehaviour
{
    Dictionary<StateType, BaseState> m_dicStates = new Dictionary<StateType, BaseState>();

    public static StateManager Instance = null;

    BaseState m_currentState = null;
    AsyncOperationHandle<SceneInstance> m_sceneLoadOperation = default;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance);

        foreach (var state in System.Enum.GetValues(typeof(StateType)) as StateType[])
        {
            _CreateState(state);
        }

        ChangeState(StateType.INIT);
    }

    public void ChangeState(StateType stateType)
    {
        if (m_dicStates.TryGetValue(stateType, out var nextState) == false)
            return;

        if (m_currentState != null)
        {
            if (m_currentState.STATE_TYPE == stateType)
                return;

            m_currentState.EndState();

            if(_IsSceneUnloadNeeded(m_currentState.STATE_TYPE))
                SceneManager.UnloadSceneAsync(m_currentState.SCENE_NAME);
        }

        m_currentState = nextState;
        if (!string.IsNullOrEmpty(m_currentState.SCENE_NAME))
        {
            m_sceneLoadOperation = Addressables.LoadSceneAsync(m_currentState.SCENE_NAME);
            m_sceneLoadOperation.Completed += GameMaster.Instance.SceneLoadCompleted;
        }
        else
        {
            m_currentState.BeginState();
        }
    }

    void _CreateState(StateType stateType)
    {
        var carrierObject = Universe.GetCarrier().GAMEOBJECT;

        BaseState state = stateType switch
        {
            StateType.INIT => carrierObject.AddComponent<InitState>(),
            StateType.LOBBY => carrierObject.AddComponent<LobbyState>(),
            StateType.TEST => carrierObject.AddComponent<TestState>(),
            _ => null
        };

        if (state == null)
            return;

        m_dicStates.Add(stateType, state);
    }

    bool _IsSceneUnloadNeeded(StateType stateType)
    {
        if (stateType == StateType.NONE ||
            stateType == StateType.INIT)
            return false;

        return true;
    }

    private void FixedUpdate()
    {
        if (m_currentState != null)
        {
            m_currentState.UpdateState();

            if (m_currentState.SCENE_LOADED == false)
            {
                if (m_sceneLoadOperation.IsValid() && m_sceneLoadOperation.IsDone)
                {
                    m_currentState.SCENE_LOADED = true;
                }
            }
        }
    }
}
