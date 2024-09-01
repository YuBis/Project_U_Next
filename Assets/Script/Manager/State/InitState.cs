using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InitState : BaseState
{
    private void Awake()
    {
        STATE_TYPE = StateType.INIT;
        SCENE_NAME = string.Empty;
    }

    public override void BeginState()
    {
        base.BeginState();

        IModel dataManager;

        dataManager = TableManager.Instance;

        dataManager = AIFactory.Instance;
        dataManager = ObjectPoolManager.Instance;
        dataManager = CombatManager.Instance;
        dataManager = KeyMappingManager.Instance;
        dataManager = StatManager.Instance;
        dataManager = UIManager.Instance;
        dataManager = CharacterManager.Instance;
        dataManager = BoardManager.Instance;

        dataManager = ServerManager.Instance;
    }

    public override void UpdateState()
    {
        base.UpdateState();

        if (TableManager.Instance.LOADING_DATA == 0)
            StateManager.Instance.ChangeState(StateType.LOBBY);
    }

    public override void EndState()
    {
        base.EndState();
    }
}
