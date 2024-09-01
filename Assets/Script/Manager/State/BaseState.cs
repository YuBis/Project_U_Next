using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState : BaseObject
{
    public StateType STATE_TYPE { get; set; } = StateType.NONE;
    public string SCENE_NAME { get; set; } = string.Empty;
    public bool SCENE_LOADED { get; set; } = false;

    bool isInited = false;

    private void OnDestroy()
    {
        SCENE_LOADED = false;
    }

    public virtual void BeginState()
    {
        isInited = true;
    }

    public virtual void UpdateState()
    {
        if(isInited == false && SCENE_LOADED == true)
        {
            BeginState();
        }
    }

    public virtual void EndState()
    {

    }
}
