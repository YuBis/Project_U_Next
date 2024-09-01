using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Collections;
using System;

public class KeyMappingManager : BaseManager<KeyMappingManager>
{
    protected override void _InitManager()
    {

    }

    public int GetHorizontalAxisKeyPressed()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            return -1;

        if (Input.GetKey(KeyCode.RightArrow))
            return 1;

        return 0;
    }

    public bool IsJumpKeyPressed()
    {
        return Input.GetKey(KeyCode.Space);
    }

    public bool IsAttackKeyPressed()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }
}