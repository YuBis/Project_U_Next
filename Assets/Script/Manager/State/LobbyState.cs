using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LobbyState : BaseState
{
    private void Awake()
    {
        STATE_TYPE = StateType.LOBBY;
        SCENE_NAME = StaticString.LOBBY_SCENE;
    }

    public override void BeginState()
    {
        base.BeginState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void EndState()
    {
        base.EndState();
    }
}
