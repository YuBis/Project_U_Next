using Spine.Unity;
using System.Collections;
using UnityEngine;

public class Player : GameCharacterView
{

    public override void OnNextInitialize()
    {
        base.OnNextInitialize();

        Physics2D.IgnoreLayerCollision(GAMEOBJECT.layer, LayerMask.NameToLayer(StaticString.MONSTER_LAYER), false);
    }

    protected override void OnDestroy()
    {
        Physics2D.IgnoreLayerCollision(GAMEOBJECT.layer, LayerMask.NameToLayer(StaticString.MONSTER_LAYER));
    }
}
