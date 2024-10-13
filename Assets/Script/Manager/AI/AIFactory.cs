using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIType
{ 
    NONE,
    PLAYER,
    AGGRESSIVE,
    NON_AGGRESSIVE,
}

public enum AIStateType
{
    NONE,
    SPAWN,
    IDLE,
    MOVE,
    PATROL,
    TARGETING,
    ATTACK,
    HIT,
    DIE,

    Count,
}

public delegate BaseAI AIFactoryDelegate(GameCharacterPresenter presenter);

public class AIFactory : BaseManager<AIFactory>
{
    Dictionary<AIType, AIFactoryDelegate> m_dicAIFactoryDelegate = new();

    protected override void _InitManager()
    {
        m_dicAIFactoryDelegate[AIType.PLAYER] = _CreatePlayerAI;
        m_dicAIFactoryDelegate[AIType.AGGRESSIVE] = _CreateAggressiveAI;
        m_dicAIFactoryDelegate[AIType.NON_AGGRESSIVE] = _CreateNonAggressiveAI;
    }

    BaseAI _CreatePlayerAI(GameCharacterPresenter presenter) => new PlayerAI(presenter);
    BaseAI _CreateAggressiveAI(GameCharacterPresenter presenter) => new AggressiveAI(presenter);
    BaseAI _CreateNonAggressiveAI(GameCharacterPresenter presenter) => new NonAggressiveAI(presenter);

    public BaseAI InjectAI(GameCharacterPresenter presenter, AIType aiType, Action<CharacterState> stateChangeFunc = null, AIStateType baseState = AIStateType.SPAWN)
    {
        if (aiType == AIType.NONE)
            return null;

        var ai = m_dicAIFactoryDelegate[aiType].Invoke(presenter);
        if(stateChangeFunc != null)
            ai.OnStateChangeRequest += stateChangeFunc;

        ai.AddNextAI(baseState);
        return ai;
    }
}
