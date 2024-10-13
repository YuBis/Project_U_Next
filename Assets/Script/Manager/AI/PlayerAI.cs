using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerAI : BaseAI
{
    public PlayerAI(GameCharacterPresenter presenter) : base(presenter)
    {
    }

    protected override async UniTask _Move()
    {
        await base._Move();

        var keyPressed = KeyMappingManager.Instance.GetHorizontalAxisKeyPressed();

        if (keyPressed != 0)
        {
            Presenter.Move(keyPressed);
        }
        else
        {
            if (!Presenter.IsJumping())
            {
                Presenter.StopMovement();
                AddNextAI(AIStateType.IDLE);
                return;
            }
        }

        if (KeyMappingManager.Instance.IsJumpKeyPressed())
        {
            Presenter.Jump();
        }

        if (KeyMappingManager.Instance.IsAttackKeyPressed())
        {
            AddNextAI(AIStateType.ATTACK);
        }
    }
    override protected async UniTask _Idle()
    {
        await base._Idle();

        if (KeyMappingManager.Instance.IsJumpKeyPressed())
        {
            Presenter.Jump();
        }

        if (KeyMappingManager.Instance.IsAttackKeyPressed())
        {
            AddNextAI(AIStateType.ATTACK);
        }
        else if (KeyMappingManager.Instance.GetHorizontalAxisKeyPressed() != 0)
        {
            AddNextAI(AIStateType.MOVE);
        }
    }

    override protected async UniTask _Attack()
    {
        await base._Attack();

        //Presenter.StopMovement();

        if (!Presenter.IsAttacking())
            AddNextAI(AIStateType.IDLE);
    }
}
