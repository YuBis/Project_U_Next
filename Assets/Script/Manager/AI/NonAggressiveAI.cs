using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class NonAggressiveAI : BaseAI
{
    public NonAggressiveAI(GameCharacterPresenter presenter) : base(presenter)
    {
    }

    override protected async UniTask _Idle()
    {
        await base._Idle();

        if (m_aiChangeTicks == 0)
        {
            m_aiChangeTicks = DateTime.Now.Ticks + TimeSpan.FromSeconds(Universe.GetDoubleRandom(1, 5)).Ticks;
        }

        if (DateTime.Now.Ticks > m_aiChangeTicks)
        {
            var dir = Universe.GetIntRandom(-1, 2);
            AddNextAI(AIStateType.PATROL, null, null, new Vector3(dir, 0, 0));
        }
    }

    override protected async UniTask _Patrol()
    {
        await base._Patrol();

        if (m_aiChangeTicks == 0)
        {
            m_aiChangeTicks = DateTime.Now.Ticks + TimeSpan.FromSeconds(Universe.GetDoubleRandom(1, 5)).Ticks;
        }

        if (m_target == null || m_target.GetState() == CharacterState.DIE || DateTime.Now.Ticks > m_aiChangeTicks || Presenter.IsFrontGroundEmpty(m_targetPos))
        {
            AddNextAI(AIStateType.IDLE);
        }
        else
        {
            Presenter.Move(m_targetPos.x);
        }
    }
}
