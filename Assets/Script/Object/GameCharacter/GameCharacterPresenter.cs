using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.PlayerLoop;
using Spine.Unity;
using Spine;
using UnityEditor.U2D.Animation;
using static UnityEngine.GraphicsBuffer;

public class GameCharacterPresenter : IMovable, IJumpable, IAttackable
{
    readonly GameCharacterModel Model;
    readonly GameCharacterView View;
    readonly BaseAI m_AI;

    public GameCharacterPresenter(GameCharacterModel model, GameCharacterView view)
    {
        View = view;
        Model = model;

        m_AI = AIFactory.Instance.InjectAI(this, Model.CharacterData?.AI_TYPE ?? AIType.NONE, OnStateChanged);
        
        _SubscribeStatUpdate();
    }

    void _SubscribeStatUpdate()
    {
        Observable.CombineLatest(
            Model.CurrentHP,
            Model.MaxHP,
            (current, max) => (current, max)
        ).Subscribe(healthValue => View.UpdateHealthUI(healthValue.current, healthValue.max))
        .AddTo(View);
    }

    public void UpdateHealthBarForce()
    {
        View.UpdateHealthUI(Model.CurrentHP.Value, Model.MaxHP.Value);
    }

    public void Update()
    {
        m_AI?.Update();
        _CheckGroundStatus();
    }

    void _CheckGroundStatus()
    {
        if(!Model.IsJumping)
            return;

        float verticalVelocity = View.RIGIDBODY.totalForce.y;
        if (verticalVelocity > 0)
            return;
            
        float groundCheckDistance = 0.1f;
        LayerMask groundLayer = LayerMask.GetMask("Ground");

        RaycastHit2D hit = Physics2D.Raycast(View.TRANSFORM.position, Vector2.down, groundCheckDistance, groundLayer);
        bool isGrounded = hit.collider != null;

        if (isGrounded)
            OnLand();
    }

    bool _CheckWallCollision(float direction)
    {
        float colliderHalfWidth = View.COLLIDER.bounds.extents.x;
        float extraDistance = 0.1f;
        float checkDistance = colliderHalfWidth + extraDistance;

        LayerMask groundLayer = LayerMask.GetMask("Ground");
        RaycastHit2D hit = Physics2D.Raycast(View.TRANSFORM.position, direction > 0 ? Vector2.right : Vector2.left, checkDistance, groundLayer);

        return hit.collider != null;
    }

    public void Move(float direction)
    {
        var force = direction * (float)Model.MoveSpeed * Time.deltaTime;
        if (_CheckWallCollision(direction))
            force = 0;

        View.ApplyMovement(force);
    }

    public void Jump()
    {
        if (Model.IsGrounded && !Model.IsJumping)
        {
            View.RIGIDBODY.AddForce(new Vector2(0, (float)Model.JumpRange));
            Model.SetJumping(true);
        }
    }

    public void OnLand()
    {
        if (!Model.IsGrounded)
        {
            Model.SetJumping(false);
        }
    }

    public void Knockback(GameCharacterPresenter thrower, float range)
    {
        var moveDir = GetPosition() - thrower.GetPosition();
        View.ApplyKnockback(moveDir.normalized * range);
    }

    public void OnDeath()
    {
        View.SetAnimationEndDelegate(delegate
        {
            CharacterManager.Instance.RemoveCharacter(this);
            View.SetHPBoard(false);
        });
    }

    public void Attack(GameCharacterPresenter target)
    {
    }

    public void UseSkill(string skillKey, params GameCharacterPresenter[] targets)
        => UseSkill(Model.GetSkill(skillKey), targets);

    public void UseSkill(SkillInstanceData skillInstance, params GameCharacterPresenter[] targets)
    {
        if (skillInstance == null)
            return;

        if (!Model.CharacterData.SKILL_LIST.Contains(skillInstance.KEY))
            return;

        if (!TableManager.Instance.SkillTable.TryGetValue(skillInstance.KEY, out var skillData))
            return;

        if (targets.Length == 0)
        {
            targets = CharacterManager.Instance.GetEnemiesInRange(this, skillData.RANGE, (int)skillInstance.GetStatValue(StatType.TARGET_COUNT)).ToArray();
            if (targets == null || targets.Length == 0)
                return;
        }

        AddDelay(skillData.RUNNING_TIME);

        Model.DecreaseMP(skillInstance.GetStatValue(StatType.MP_COST));
        Model.DecreaseHP(skillInstance.GetStatValue(StatType.HP_COST));

        if (targets.Length > 0)
        {
            foreach (var target in targets)
            {
                CombatManager.Instance.ProcessApplySkill(this, target, skillInstance);
            }
        }
    }

    public void HandleGrounded()
    {
        OnLand();
    }

    public void BodyCheckProcess(GameObject collisionObject)
    {
        var oppositePresenter = collisionObject.GetComponent<GameCharacterView>().Presenter;
        if (oppositePresenter == null || oppositePresenter.GetTeamType() == GetTeamType())
            return;

        if (Model.CharacterData.BODY_CHECK)
        {
            var bodyCheckSkill = GetSkill(SkillType.BODY_CHECK);
            UseSkill(bodyCheckSkill?.SKILL_TEMPLATE?.KEY, oppositePresenter);
        }
    }

    float checkInterval = 0.1f;
    float lastCheckTime = 0;
    int groundLayerMask = LayerMask.GetMask(StaticString.GROUND_LAYER);
    bool isFrontEmptyBefore = false;

    public bool IsFrontGroundEmpty(Vector2 targetPosition)
    {
        if (Time.time < lastCheckTime + checkInterval)
            return isFrontEmptyBefore;

        lastCheckTime = Time.time;

        //View.SetVelocity(new Vector2(targetPosition.x, View.RIGIDBODY.velocity.y));

        var targetDir = targetPosition.x > View.RIGIDBODY.position.x ? Vector2.right : Vector2.left;
        Vector2 nextBlock = new Vector2(View.RIGIDBODY.position.x + targetDir.x * 0.5f, View.RIGIDBODY.position.y + 0.5f);
        Debug.DrawRay(nextBlock, Vector3.down, Color.green);

        RaycastHit2D raycast = Physics2D.Raycast(nextBlock, Vector2.down, 1, groundLayerMask);
        isFrontEmptyBefore = raycast.collider == null;

        return isFrontEmptyBefore;
    }

    public void AddDelay(double delay)
    {
        if (delay == 0)
            return;

        Model.AddDelay(delay);
    }

    public SkillInstanceData GetSkill(string skillKey)
    {
        if (string.IsNullOrEmpty(skillKey))
            return null;

        return Model.GetSkill(skillKey);
    }

    public SkillInstanceData GetSkill(SkillType skillType)
    {
        return Model.GetSkill(skillType);
    }

    public void DecreaseHP(double amount)
    {
        Model.DecreaseHP(amount);
    }

    public void DecreaseMP(double amount)
    {
        Model.DecreaseMP(amount);
    }

    public void OnStateChanged(CharacterState state)
    {
        Model.SetState(state);
    }

    public void UpdatePosition(Vector2 pos)
    {
        Model.SetPosition(pos);
    }

    public void SetPosition(Vector2 pos)
    {
        View.UpdatePosition(pos);
    }

    public double GetDelay()
    {
        return Model.Delay;
    }

    public void HandleAnimationStart(TrackEntry trackEntry)
    {
        if (Model.AIState == AIStateType.ATTACK)
            Model.SetAttackStatus(true);
    }

    public void HandleAnimationComplete(TrackEntry trackEntry)
    {
        if (Model.AIState == AIStateType.ATTACK)
            Model.SetAttackStatus(false);
    }

    public void HandleAnimationEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (Model.AIState == AIStateType.ATTACK)
            UseSkill(GetSkill(SkillType.ATTACK_NORMAL));
    }

    public void StopMovement()
    {
        View.StopMovement();
    }

    public bool IsJumping() => Model.IsJumping;
    public bool IsAttacking() => Model.IsAttacking;
    public Vector2 GetPosition() => Model.Position;
    public double GetStat(StatType type) => Model.StatData.GetStat(type);
    public CharacterState GetState() => Model.State;
    public Vector3 GetScale() => View.GetScale();
    public Quaternion GetRotation() => View.GetRotation();
    public TeamType GetTeamType() => Model.Team;
    public string GetTemplateKey() => Model.TemplateKey;

    public SkeletonAnimation GetSkeletonAnimation() => View.SkeletonAnimation;
    public string GetAnimationName() => Model.GetAnimationName();
    public string GetSubAnimationName() => Model.GetSubAnimationName();
    public void ChangeAnimation(bool isLoop = true) => View.ChangeAnimation(isLoop);
    public GameObject GetGameObject() => View.GAMEOBJECT;
    public BaseAI GetAI() => m_AI;
    public Vector2 GetBoardPos() => View.GetBoardPos();

    public double GetCurrentHP() => Model.CurrentHP.Value;
    public double GetCurrentMP() => Model.CurrentMP.Value;
    public bool IsGrounded() => Model.IsGrounded;
    public void SetAIState(AIStateType aiState) => Model.SetAIState(aiState);

    public void SetParent(Transform parent) => View.TRANSFORM.SetParent(parent, false);
}
