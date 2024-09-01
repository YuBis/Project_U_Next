using Com.LuisPedroFonseca.ProCamera2D.TopDownShooter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using Spine;
using Cysharp.Threading.Tasks;

public struct stNextAI
{
    public AIStateType m_nextState;
    public GameCharacterPresenter m_targetChar;
    public Vector3 m_targetPos;
    public string m_nextSkill;
}

public class BaseAI
{
    protected AIStateType m_beforeAIState = AIStateType.NONE;
    protected AIStateType m_currentAIState = AIStateType.NONE;

    delegate UniTask UpdateFunc();
    delegate void ChangeFunc();

    readonly UpdateFunc[] m_arrUpdateFunc = new UpdateFunc[(int)AIStateType.Count];
    readonly ChangeFunc[] m_arrChangeFunc = new ChangeFunc[(int)AIStateType.Count];

    protected Queue<stNextAI> m_listNextAI = new();

    protected GameCharacterPresenter m_target = null;
    protected Vector3 m_targetPos;

    protected double m_aiChangeTicks = 0;

    public Vector3 SpawnPos { get; protected set; }
    public GameCharacterPresenter Presenter { get; protected set; } = null;
    public event Action<CharacterState> OnStateChangeRequest;

    public bool m_isAttack = false;

    public BaseAI(GameCharacterPresenter presenter)
    {
        Presenter = presenter;

        _InitializeUpdateFuncs();
        _InitializeChangeFuncs();
        _InitializeAI();
    }

    void _InitializeAI()
    {
        m_listNextAI.Clear();
        m_target = null;
        m_targetPos = Vector3.zero;
        SpawnPos = Presenter.GetPosition();
    }

    void _InitializeUpdateFuncs()
    {
        m_arrUpdateFunc[(int)AIStateType.SPAWN] = _Spawn;
        m_arrUpdateFunc[(int)AIStateType.IDLE] = _Idle;
        m_arrUpdateFunc[(int)AIStateType.MOVE] = _Move;
        m_arrUpdateFunc[(int)AIStateType.PATROL] = _Patrol;
        m_arrUpdateFunc[(int)AIStateType.TARGETING] = _Targeting;
        m_arrUpdateFunc[(int)AIStateType.ATTACK] = _Attack;
        m_arrUpdateFunc[(int)AIStateType.HIT] = _Hit;
        m_arrUpdateFunc[(int)AIStateType.DIE] = _Die;
    }

    void _InitializeChangeFuncs()
    {
        m_arrChangeFunc[(int)AIStateType.SPAWN] = _ToSpawn;
        m_arrChangeFunc[(int)AIStateType.MOVE] = _ToMove;
        m_arrChangeFunc[(int)AIStateType.IDLE] = _ToIdle;
        m_arrChangeFunc[(int)AIStateType.PATROL] = _ToPatrol;
        m_arrChangeFunc[(int)AIStateType.TARGETING] = _ToTargeting;
        m_arrChangeFunc[(int)AIStateType.ATTACK] = _ToAttack;
        m_arrChangeFunc[(int)AIStateType.HIT] = _ToHit;
        m_arrChangeFunc[(int)AIStateType.DIE] = _ToDie;
    }

    virtual protected bool SyncDirForSkillUse()
    {
        return true;
    }

    virtual protected async UniTask _Spawn()
    {
        AddNextAI(AIStateType.IDLE);
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Idle()
    {
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Move()
    {
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Attack()
    {
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Patrol()
    {
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Targeting()
    {
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Hit()
    {
        AddNextAI(AIStateType.IDLE);
        await UniTask.NextFrame();
    }

    virtual protected async UniTask _Die()
    {
        await UniTask.NextFrame();
    }

    virtual protected void _ToSpawn()
    {
        _ChangeCharacterState(CharacterState.SPAWNING);
        Presenter.ChangeAnimation(false);
    }

    virtual protected void _ToIdle()
    {
        _ChangeCharacterState(CharacterState.NORMAL);
        Presenter.ChangeAnimation();
    }

    virtual protected void _ToMove()
    {
        _ChangeCharacterState(CharacterState.NORMAL);
        Presenter.ChangeAnimation();
    }

    virtual protected void _ToPatrol()
    {
        _ChangeCharacterState(CharacterState.NORMAL);
        Presenter.ChangeAnimation();
    }

    virtual protected void _ToTargeting()
    {
        _ChangeCharacterState(CharacterState.NORMAL);
        Presenter.ChangeAnimation();
    }

    virtual protected void _ToAttack()
    {
        _ChangeCharacterState(CharacterState.NORMAL);
        Presenter.ChangeAnimation(false);
    }

    virtual protected void _ToHit()
    {
        _ChangeCharacterState(CharacterState.NORMAL);
        Presenter.ChangeAnimation(false);
    }

    virtual protected void _ToDie()
    {
        _ChangeCharacterState(CharacterState.DIE);

        Presenter.ChangeAnimation(false);
        Presenter.OnDeath();
    }

    virtual public void AddNextAI(AIStateType nextStateType, GameCharacterPresenter targetChar = null, string skillKey = null, Vector3 targetPos = new Vector3())
    {
        stNextAI nextAI = new stNextAI
        {
            m_nextState = nextStateType,
            m_targetChar = targetChar,
            m_targetPos = targetPos,
            m_nextSkill = skillKey
        };

        m_listNextAI.Enqueue(nextAI);
    }

    void _ChangeAI(stNextAI nextAI)
    {
        m_beforeAIState = m_currentAIState;
        m_currentAIState = nextAI.m_nextState;

        if(m_currentAIState == AIStateType.TARGETING)
        {
            m_target = nextAI.m_targetChar;
            m_targetPos = m_target?.GetPosition() ?? Presenter.GetPosition();
        }
        else
        {
            m_targetPos = nextAI.m_targetPos;
        }

        m_aiChangeTicks = 0;

        Presenter.SetAIState(nextAI.m_nextState);

        m_arrChangeFunc[(int)nextAI.m_nextState]?.Invoke();
    }

    public void Update()
    {
        if (m_listNextAI.Count > 0)
        {
            _ChangeAI(m_listNextAI.Dequeue());
        }

        m_arrUpdateFunc[(int)m_currentAIState]?.Invoke();
    }

    protected async UniTask _WaitDelay()
    {
        if (Presenter == null)
            return;

        while (Presenter.GetDelay() > 0)
        {
            Presenter.AddDelay(-Time.deltaTime);
            await UniTask.NextFrame();
        }
    }

    protected void _ChangeCharacterState(CharacterState state)
    {
        OnStateChangeRequest?.Invoke(state);
    }
}