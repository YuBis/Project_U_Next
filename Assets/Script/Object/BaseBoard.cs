using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBoard : BaseObject
{
    public virtual BoardType BOARD_TYPE => BoardType.NONE;

    public GameCharacterPresenter OWNER { get; set; } = null;

    public virtual void OnRelease()
    {

    }

    public virtual void Refresh(double value1, double value2 = 0)
    {

    }

    private void FixedUpdate()
    {
        if (OWNER == null)
            return;

        TRANSFORM.position = BoardManager.Instance.GetUIPosition(OWNER.GetBoardPos());
    }
}
