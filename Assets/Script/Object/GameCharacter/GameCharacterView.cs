using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class GameCharacterView : BaseObject
{
    [SerializeField] Transform m_boardPos;

    HPBoard m_hpBoard;
    Vector3 Velocity;

    public SkeletonAnimation SkeletonAnimation { get; private set; }

    public GameCharacterPresenter Presenter { get; private set; }

    void Awake()
    {
        SkeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    private void Update()
    {
        if (TRANSFORM.hasChanged)
        {
            Presenter.UpdatePosition(TRANSFORM.position);
            TRANSFORM.hasChanged = false;
        }

        Presenter.Update();
    }

    protected virtual void OnDestroy()
    {
        if (SkeletonAnimation != null)
        {
            SkeletonAnimation.AnimationState.Start -= OnAnimationStart;
            SkeletonAnimation.AnimationState.Event -= OnAnimationEvent;
            SkeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
        }
    }

    public void Initialize(GameCharacterPresenter presenter)
    {
        Presenter = presenter;

        SetHPBoard();

        OnNextInitialize();
    }

    public virtual void OnNextInitialize()
    {
        if (SkeletonAnimation != null)
        {
            SkeletonAnimation.AnimationState.Start += OnAnimationStart;
            SkeletonAnimation.AnimationState.Event += OnAnimationEvent;
            SkeletonAnimation.AnimationState.Complete += OnAnimationComplete;
        }
    }

    void OnAnimationStart(TrackEntry trackEntry)
    {
        Presenter.HandleAnimationStart(trackEntry);
    }

    void OnAnimationComplete(TrackEntry trackEntry)
    {
        Presenter.HandleAnimationComplete(trackEntry);

    }

    void OnAnimationEvent(TrackEntry trackEntry, Spine.Event e)
    {
        Presenter.HandleAnimationEvent(trackEntry, e);
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        TRANSFORM.position = newPosition;
    }

    public void UpdateHealthUI(double curValue, double maxValue)
    {
        if (m_hpBoard != null)
            m_hpBoard.Refresh(curValue, maxValue);
    }

    public void SetRotation(float yRotation)
    {
        TRANSFORM.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    public async UniTaskVoid ApplyMovement(float direction, Vector2 targetPosition, float speed, CancellationToken cancellationToken)
    {
        SetRotation(direction > 0 ? 0 : 180);

        while (Vector2.Distance(TRANSFORM.position, targetPosition) > speed)
        {
            if (cancellationToken.IsCancellationRequested)
                return; // 이동 취소

            UpdatePosition(Vector2.MoveTowards(TRANSFORM.position, targetPosition, speed));
            await UniTask.NextFrame(cancellationToken);
        }

        UpdatePosition(targetPosition);
    }

    public void ApplyJump(float jumpForce)
    {
        RIGIDBODY.AddForce(new Vector2(0, jumpForce));
    }

    public bool IsTouchingWall(float direction)
    {
        float colliderHalfWidth = COLLIDER.bounds.extents.x;
        float extraDistance = 0.1f;
        float checkDistance = colliderHalfWidth + extraDistance;

        LayerMask groundLayerMask = 1 << LayerMask.NameToLayer(StaticString.GROUND_LAYER);
        RaycastHit2D hit = Physics2D.Raycast(TRANSFORM.position, direction > 0 ? Vector2.right : Vector2.left, checkDistance, groundLayerMask);

        Debug.DrawRay(TRANSFORM.position, (direction > 0 ? Vector2.right : Vector2.left) * checkDistance, Color.red, 0.1f);

        return hit.collider != null;
    }


    public void StopMovement()
    {
        SetVelocity(new Vector2(0, RIGIDBODY.velocity.y));
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        RIGIDBODY.velocity = newVelocity;
    }

    public void ApplyKnockback(Vector2 force)
    {
        RIGIDBODY.AddForce(force);
    }

    public void ChangeAnimation(bool isLoopAni = true)
    {
        if (SkeletonAnimation == null)
            return;

        var aniName = Presenter.GetAnimationName();

        if (SkeletonAnimation.Skeleton != null)
        {
            if (SkeletonAnimation.Skeleton.Data.FindAnimation(aniName) == null)
            {
                aniName = Presenter.GetSubAnimationName();
                if (SkeletonAnimation.Skeleton.Data.FindAnimation(aniName) == null)
                    return;
            }
        }

        SkeletonAnimation.loop = isLoopAni;
        SkeletonAnimation.AnimationState.SetAnimation(0, aniName, isLoopAni);
    }

    public void SetHPBoard(bool show = true)
    {
        if (!show)
        {
            BoardManager.Instance.ReleaseBoard(BoardType.HP, m_hpBoard);
            m_hpBoard = null;
            return;
        }

        if (m_hpBoard == null)
        {
            BoardManager.Instance.MakeBoard(Presenter, BoardType.HP, (board) =>
            {
                m_hpBoard = board as HPBoard;
                Presenter.UpdateHealthBarForce();
            });
        }
    }

    public void SetAnimationEndDelegate(Spine.AnimationState.TrackEntryDelegate trackDlg)
    {
        SkeletonAnimation.AnimationState.End += trackDlg;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        var collisionObject = collision.gameObject;
        if (collisionObject == null)
            return;

        if (collisionObject.CompareTag(StaticString.MAP))
        {
            //if (IsGroundContact(collision))
            //{
            //Presenter.HandleGrounded();
            //}
        }
        else
        {
            Presenter.BodyCheckProcess(collisionObject);
        }
    }

    public bool IsGroundContact(Collision2D collision)
    {
        return collision.contacts.Any(contact => contact.normal.y > 0.5);
    }

    public Vector3 GetScale() => TRANSFORM.localScale;
    public Quaternion GetRotation() => TRANSFORM.localRotation;
    public Vector2 GetBoardPos() => m_boardPos.position;
}