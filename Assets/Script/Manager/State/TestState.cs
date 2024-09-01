using UnityEngine.AddressableAssets;
using System.Collections;
using UnityEngine;

public class TestState : BaseState
{
    private void Awake()
    {
        STATE_TYPE = StateType.TEST;
        SCENE_NAME = StaticString.TEST_SCENE;
    }

    public override void BeginState()
    {
        base.BeginState();

        UIManager.Instance.MakeUI("Pf_Ui_Game");
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