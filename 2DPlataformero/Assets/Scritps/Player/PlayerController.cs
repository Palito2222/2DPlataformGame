using Cinemachine;
using UnityEngine;

public class PlayerController : FSM
{
    public enum FSMState
    {
        None, Idle, Moving, Jump, Fall, Attack, Hit, Dead
    }

    public FSMState curState = FSMState.None;

    [SerializeField] private CharacterData characterData;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private Transform attackPosOrigin;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Animator animator;
    private Rigidbody2D rb;

    private float horizontalAxis;
    private bool jump = false;
    private bool attack = false;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        virtualCamera.Follow = transform;
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Initialize()
    {
        SetDefaultState();
    }

    private void SetDefaultState()
    {
        curState = FSMState.Idle;
    }

    protected override void FSMUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        horizontalAxis = Input.GetAxis("Horizontal");
        jump = Input.GetButton("Jump");
        attack = Input.GetButtonDown("Fire1");
    }

    protected override void FSMFixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        switch (curState)
        {
            case FSMState.Idle:
                UpdateIdleState();
                break;
            case FSMState.Moving:
                UpdateMovingState();
                break;
            case FSMState.Jump:
                UpdateJumpState();
                break;
            case FSMState.Fall:
                UpdateFallState();
                break;
            case FSMState.Attack:
                UpdateAttackState();
                break;
            case FSMState.Hit:
                UpdateHitState();
                break;
            case FSMState.Dead:
                UpdateDeadState();
                break;
        }
    }

    private void UpdateIdleState()
    {
        // Actualizar Estado

        // animator.Play("CharacterIdleAnimation");

        // Resetear la velocidad a cero
        rb.velocity = new Vector2(0f, rb.velocity.y);

        // Reglas de Transición

        if (jump)
        {
            curState = FSMState.Jump;
            return;
        }

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        if (Mathf.Abs(horizontalAxis) > Mathf.Epsilon)
        {
            if (IsOnSlope() && Mathf.Abs(GetSlopeAngle()) <= characterData.maxSlopeAngle)
            {
                float slopeVelocity = Mathf.MoveTowards(rb.velocity.x, horizontalAxis * characterData.moveSpeed, characterData.slopeAcceleration);
                rb.velocity = new Vector2(slopeVelocity, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);
            }
            curState = FSMState.Moving;
        }
    }

    private void UpdateMovingState()
    {
        // Actualizar Estado

        // animator.Play("CharacterMovingAnimation");

        if (IsOnSlope() && Mathf.Abs(GetSlopeAngle()) <= characterData.maxSlopeAngle)
        {
            float slopeVelocity = Mathf.MoveTowards(rb.velocity.x, horizontalAxis * characterData.moveSpeed, characterData.slopeAcceleration);
            rb.velocity = new Vector2(slopeVelocity, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);

            if (jump && IsGrounded() && Mathf.Abs(GetSlopeAngle()) <= characterData.maxSlopeAngle)
            {
                rb.velocity = new Vector2(rb.velocity.x, characterData.jumpSpeed);
            }
        }

        // Reglas de Transición

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        if ((!IsGrounded() && !IsOnSlope()) || (IsOnSlope() && Mathf.Abs(GetSlopeAngle()) > characterData.maxSlopeAngle))
        {
            curState = FSMState.Fall;
        }

        if (Mathf.Abs(horizontalAxis) < Mathf.Epsilon && IsGrounded() && !IsOnSlope())
        {
            curState = FSMState.Idle;
        }
    }

    private void UpdateJumpState()
    {
        // Actualizar Estado

        // animator.Play("CharacterJumpAnimation");

        rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);

        if (jump && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, characterData.jumpSpeed);
        }

        // Reglas de Transición

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        if (rb.velocity.y < 0)
        {
            curState = FSMState.Fall;
        }
    }

    private void UpdateFallState()
    {
        // Actualizar Estado
        // animator.Play("CharacterFallAnimation")

        float targetVelocityX = horizontalAxis * characterData.moveSpeed;

        if (IsOnSlope() && Mathf.Abs(GetSlopeAngle()) > characterData.maxSlopeAngle)
        {
            // Resbalar automáticamente hacia abajo en pendientes pronunciadas
            targetVelocityX = 0f;
        }

        rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);

        // Reglas de Transición

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        if ((IsGrounded() && !IsOnSlope() && rb.velocity.y <= 0) || (IsOnSlope() && Mathf.Abs(GetSlopeAngle()) < characterData.maxSlopeAngle && Mathf.Abs(rb.velocity.x) < 0.1f && rb.velocity.y <= 0 && Mathf.Abs(GetSlopeAngle()) <= characterData.maxSlopeAngle))
        {
            if (Mathf.Abs(horizontalAxis) > Mathf.Epsilon)
            {
                curState = FSMState.Moving;
            }
            else
            {
                curState = FSMState.Idle;
            }
        }

        // Declaraciones de depuración
        Debug.Log("Slope Angle: " + GetSlopeAngle());
        Debug.Log("Max Slope Angle: " + characterData.maxSlopeAngle);
    }

    private void UpdateAttackState()
    {
        // Actualizar Estado

        // if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttackAnimation")) 
        // {
        //     animator.Play("PlayerAttackAnimation");
        //
        //     Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPosOrigin.position, characterData.attackRadius, LayerMask.GetMask("Enemy"));
        //
        //     foreach (Collider2D enemy in enemies)
        //     {
        //         // Hacer Daño
        //     }
        // }

        // Reglas de Transición

        // if (animator.GetAnimatorTransitionInfo(0).normalizedTime > 0.9f)
        // {
        //     if (IsGrounded())
        //     {
        //         curState = FSMState.Idle;
        //     }
        //
        //     if (rb.velocity.y > 0)
        //     {
        //         curState = FSMState.Fall;
        //     }
        // }
    }

    private void UpdateHitState()
    {
        // Actualizar Estado

        // animator.Play("CharacterHitAnimation");
        // Añadir fuerza al hit

        // Reglas de Transición

        if (IsGrounded())
        {
            curState = FSMState.Idle;
        }

        if (rb.velocity.y < 0)
        {
            curState = FSMState.Fall;
        }
    }

    private void UpdateDeadState()
    {
        // Actualizar Estado

        // animator.Play("CharacterDeadAnimation");
        // isPlayerDead = true;

        // Reglas de Transición
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.5f, groundLayer);
        return hit.collider != null;
    }

    private bool IsOnSlope()
    {
        RaycastHit2D slopeHit = Physics2D.Raycast(groundCheck.position, Vector2.down, 1.0f, groundLayer);
        return slopeHit.collider != null && Mathf.Abs(slopeHit.normal.x) > 0.1f;
    }

    private float GetSlopeAngle()
    {
        RaycastHit2D slopeHit = Physics2D.Raycast(groundCheck.position, Vector2.down, 1.0f, groundLayer);
        return Mathf.Atan2(slopeHit.normal.x, slopeHit.normal.y) * Mathf.Rad2Deg;
    }

    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
    }
}
