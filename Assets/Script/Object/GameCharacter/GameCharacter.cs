//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using static UnityEngine.EventSystems.EventTrigger;
//using static UnityEngine.GraphicsBuffer;

//public class GameCharacter : BaseObject
//{
//    [SerializeField] Transform m_boardPos = null;

//    StatSheetData m_statSheetData = null;
//    CharacterTemplateData m_characterTemplateData = null;

//    protected bool m_bMove;
//    protected bool m_bIsJumping;
//    protected bool m_bGrap;
//    protected bool m_bChangeDirection;
//    protected bool m_bActionable;
//    protected bool m_bCanHit;
//    protected bool m_facingRight = true;
//    protected bool m_bIsGrounded;
//    protected bool m_bIsJumpingUp = false;

//    protected double m_jumpRange = 10;
//    protected double m_moveSpeed = 100;

//    protected List<SkillInstanceData> m_listSkillData = null;

//    public bool IS_JUMPING => m_bIsJumping;
//    public double JUMP_RANGE => m_jumpRange;
//    public double MOVE_SPEED => m_moveSpeed;

//    protected Vector3 m_Velocity = Vector3.zero;

//    protected float m_groundedRadius = .2f;
//    protected float m_movementSmoothing = .05f;

//    public string TEMPLATE_KEY { get; private set; }
//    public CharacterTemplateData CHARACTER_DATA => m_characterTemplateData;
//    public StatSheetData STAT_SHEET_DATA => m_statSheetData;
//    public StatData STAT_DATA { get; private set; }
//    public BaseAI AI { get; private set; }
//    public TeamType TEAM_TYPE { get; private set; }
//    public CharacterState CHARACTER_STATE { get; set; }
//    public int LEVEL { get; private set; }
//    public int EXP { get; protected set; }
//    public double CURRENT_HP { get; protected set; }
//    public double CURRENT_MP { get; protected set; }
//    public ReadOnlyCollection<SkillInstanceData> LIST_SKILL_INSTANCE => m_listSkillData.AsReadOnly();

//    public double DELAY { get; set; }
//    public Transform BOARD_POS => m_boardPos;

//    HPBoard m_hpBoard = null;


//    float m_jumpCheckDelay = 0;
//    protected bool m_bIsInitialized = false;

//    private void OnEnable()
//    {
//        m_bIsInitialized = false;
//    }

//    virtual public void Initialize(string templateKey, TeamType teamType)
//    {
//        TEMPLATE_KEY = templateKey;
//        TEAM_TYPE = teamType;

//        if (!TableManager.Instance.CharacterTable.TryGetValue(TEMPLATE_KEY, out m_characterTemplateData))
//        {
//            Universe.LogError(TEMPLATE_KEY + " : Character template could not found!");
//            return;
//        }

//        TableManager.Instance.StatSheetTable.TryGetValue(CHARACTER_DATA.STAT_SHEET, out m_statSheetData);

//        LEVEL = 1;
//        EXP = 0;

//        STAT_DATA = StatManager.Instance.GetCurrentStat(in m_statSheetData, LEVEL);

//        AI = AIFactory.Instance.InjectAI(this, CHARACTER_DATA.AI_TYPE);

//        CHARACTER_STATE = CharacterState.SPAWNING;

//        CURRENT_HP = STAT_DATA.GetStat(StatType.HP);
//        CURRENT_MP = STAT_DATA.GetStat(StatType.MP);

//        m_jumpRange = STAT_DATA.GetStat(StatType.JUMP_RANGE);
//        m_moveSpeed = STAT_DATA.GetStat(StatType.MOVE_SPEED);

//        m_listSkillData = SkillInstanceManager.Instance.MakeDefaultSkills(CHARACTER_DATA);

//        SetHPBoard();

//        m_bIsInitialized = true;
//    }

//    public virtual void UseSkill(SkillInstanceData skillInstance, params GameCharacter[] targets)
//    {
//        if (!m_bIsInitialized)
//            return;

//        if (skillInstance == null)
//            return;

//        if (!CHARACTER_DATA.SKILL_LIST.Contains(skillInstance.KEY))
//            return;

//        if (!TableManager.Instance.SkillTable.TryGetValue(skillInstance.KEY, out var skillData))
//            return;

//        if (targets.Length == 0)
//        {
//            targets = CharacterManager.Instance.GetEnemiesInRange(this, skillData.RANGE, (int)skillInstance.GetStatValue(StatType.TARGET_COUNT)).ToArray();
//            if (targets == null || targets.Length == 0)
//                return;
//        }

//        DELAY += skillData.RUNNING_TIME;

//        DecreaseHP(skillInstance.GetStatValue(StatType.HP_COST));
//        DecreaseMP(skillInstance.GetStatValue(StatType.MP_COST));

//        foreach (var target in targets)
//        {
//            CombatManager.Instance.ProcessApplySkill(this, target, skillInstance);
//        }
//    }

//    public virtual void UseSkill(string skillKey, params GameCharacter[] targets)
//        => UseSkill(LIST_SKILL_INSTANCE.FirstOrDefault(x => x.SKILL_TEMPLATE.KEY == skillKey), targets);

//    public void Knockback(GameCharacter thrower, float range)
//    {
//        var moveDir = TRANSFORM.position - thrower.TRANSFORM.position;
//        RIGIDBODY.AddForce(moveDir.normalized * range);
//    }

//    public virtual void OnDeath()
//    {
//        BoardManager.Instance.ReleaseBoard(BoardType.HP, m_hpBoard);
//        m_hpBoard = null;
//    }

//    public void Jump()
//    {
//        if (!m_bIsGrounded)
//            return;

//        RIGIDBODY.AddForce(new Vector2(0, (float)JUMP_RANGE));
//        m_bIsJumping = true;
//        m_bIsGrounded = false;
//        m_jumpCheckDelay = Time.time + 0.1f;
//    }

//    public virtual void OnLand()
//    {
//        m_bIsJumping = false;
//    }

//    public virtual void Flip()
//    {
//        m_facingRight = !m_facingRight;

//        var rot = TRANSFORM.localRotation;
//        rot.y = (rot.y == 0 ? 180 : 0);
//        TRANSFORM.localRotation = rot;
//    }

//    protected virtual void OnCollisionEnter2D(Collision2D collision)
//    {
//        var collisionObject = collision.gameObject;
//        if (collisionObject == null)
//            return;

//        if (!m_bIsGrounded && Time.time > m_jumpCheckDelay)
//        {
//            if (collisionObject.tag == StaticString.MAP)
//            {
//                m_bIsGrounded = true;
//                OnLand();
//            }
//        }

//        if(!collisionObject.CompareTag("MAP"))
//            _BodyCheckProcess(collisionObject);
//    }

//    void _BodyCheckProcess(GameObject collisionObject)
//    {
//        var opposite = collisionObject.GetComponent<GameCharacter>();
//        if (opposite == null)
//            return;

//        if (opposite.TEAM_TYPE == TEAM_TYPE)
//            return;

//        if (!CHARACTER_DATA.BODY_CHECK)
//            return;

//        var bodyCheckSkill = LIST_SKILL_INSTANCE.FirstOrDefault(x => x.SKILL_TEMPLATE.TYPE == SkillType.BODY_CHECK);
//        UseSkill(bodyCheckSkill?.SKILL_TEMPLATE?.KEY, opposite);
//    }

//    public void DecreaseHP(double damage)
//    {
//        CURRENT_HP -= damage;
//        m_hpBoard?.Refresh();
//    }

//    public void DecreaseMP(double mpValue)
//    {
//        CURRENT_MP -= mpValue;
//    }

//    public double GetStatValue(StatType statType)
//    {
//        if (m_statSheetData == null)
//            return 0;

//        if (!m_statSheetData.DIC_INFO.TryGetValue(statType, out var targetStat))
//            return 0;

//        return targetStat.VALUE + (targetStat.VALUE_PER_LV * (LEVEL - 1));
//    }

//    public void SetHPBoard(bool show = true)
//    {
//        if(!show)
//        {
//            BoardManager.Instance.ReleaseBoard(BoardType.HP, m_hpBoard);
//            m_hpBoard = null;
//            return;
//        }

//        if(m_hpBoard == null)
//        {
//            BoardManager.Instance.MakeBoard(this, BoardType.HP, (board) =>
//            {
//                m_hpBoard = board as HPBoard;
//                if (m_hpBoard == null)
//                    return;

//                m_hpBoard.SetBoardData();
//            });
//        }
//        else
//            m_hpBoard.SetBoardData();


//    }
//}