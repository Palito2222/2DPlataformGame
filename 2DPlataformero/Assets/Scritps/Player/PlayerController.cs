using Cinemachine;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerController : FSM
{
    public enum FSMState
    {
        None, Idle, Moving, Jump, Fall, Attack, Hit, Dead
    }

    public FSMState curState = FSMState.None;

    //Variables y Referencias
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterData characterData;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform characterSprite; // Referencia al objeto vacío que contiene el sprite del personaje
    private Rigidbody2D rb;

    [SerializeField] private Transform attackPosOrigin;

    //GroundChecks
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float slideSpeed = 2.0f;

    //Input
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

        // Cambiar la rotación del sprite del personaje según la dirección horizontal
        int lookAt = horizontalAxis > 0 ? -1 : horizontalAxis < 0 ? 1 : (int)characterSprite.localScale.x;
        characterSprite.localScale = new Vector2(lookAt, 1);
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

        float slopeAngle = GetSlopeAngle();

        if (IsGrounded())
        {
            if (Mathf.Abs(slopeAngle) < characterData.maxSlopeAngle)
            {
                //Volver Kinematico al personaje, para ignorar las fisicas (ej: rampas)
                rb.isKinematic = true;
            }
            // Resetear la velocidad a cero
            rb.velocity = Vector2.zero;
        }



        // Reglas de Transición
        if (jump && IsGrounded())
        {
            rb.isKinematic = false;
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
            if (rb.isKinematic == true)
            {
                rb.isKinematic = false;
            }

            curState = FSMState.Moving;
            return;
        }

        if (!IsGrounded())
        {
            if (rb.isKinematic == true)
            {
                rb.isKinematic = false;
            }

            curState = FSMState.Fall;
            return;
        }
    }

    private void UpdateMovingState()
    {
        // Actualizar Estado

        // animator.Play("CharacterMovingAnimation");

        float slopeAngle = GetSlopeAngle();

        //Movimiento basado en Velocidad Constante si el personaje está en el suelo
        if (IsGrounded() || (IsGrounded() && Mathf.Abs(slopeAngle) < characterData.maxSlopeAngle))
        {
            rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);
        }

        // Reglas de Transición

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        if (jump && IsGrounded())
        {
            curState = FSMState.Jump;
            return;
        }

        if (!IsGrounded() || Mathf.Abs(slopeAngle) > characterData.maxSlopeAngle)
        {
            curState = FSMState.Fall;
            return;

        }

        if (Mathf.Abs(horizontalAxis) < Mathf.Epsilon && IsGrounded())
        {
            curState = FSMState.Idle;
            return;
        }
    }

    private void UpdateJumpState()
    {
        // Actualizar Estado

        // animator.Play("CharacterJumpAnimation");

        float slopeAngle = GetSlopeAngle();

        // Mantener la velocidad horizontal del estado Moving
        rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);

        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, characterData.jumpSpeed);
        }

        // Reglas de Transición

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        //Si el personaje está a una altura menor que un valor cercano a 0
        if (rb.velocity.y < Mathf.Epsilon || Mathf.Abs(slopeAngle) > characterData.maxSlopeAngle)
        {
            curState = FSMState.Fall;
            return;
        }

        //Si el jugador está en el suelo y su input Horizontal es mayor que un valor cercano a 0 (ej: 0.1), cambio a Moving
        if (Mathf.Abs(horizontalAxis) > Mathf.Epsilon && IsGrounded())
        {
            curState = FSMState.Moving;
            return;
        }
    }

    private void UpdateFallState()
    {
        // Actualizar Estado
        // animator.Play("CharacterFallAnimation")

        bool isSliding = false; // Variable para realizar un seguimiento del estado de deslizamiento
        float slopeAngle = GetSlopeAngle();

        if (Mathf.Abs(slopeAngle) > characterData.maxSlopeAngle)
        {
            // Calcular la velocidad de deslizamiento basada en la pendiente y el signo del ángulo
            float slideSpeedRB = slideSpeed * Mathf.Cos(Mathf.Deg2Rad * slopeAngle) * Mathf.Sign(slopeAngle);

            // Aplicar la velocidad de deslizamiento
            rb.velocity = new Vector2(slideSpeedRB, rb.velocity.y);

            // Establecer el estado de deslizamiento en verdadero
            isSliding = true;
        }
        else
        {
            if (!isSliding)
            {
                rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);
            }
            else if (IsGrounded())
            {
                // Restablecer el estado de deslizamiento a falso
                isSliding = false;
            }
        }


        // Reglas de Transición

        //if (attack)
        //{
        //    curState = FSMState.Attack;
        //    return;
        //}

        //Si el jugador está en el suelo y su input Horizontal es menor que un valor cercano a 0 (ej: -0.1), cambio a Idle
        if (Mathf.Abs(horizontalAxis) < Mathf.Epsilon && IsGrounded())
        {
            curState = FSMState.Idle;
            return;
        }

        //Si el jugador está en el suelo y su input Horizontal es mayor que un valor cercano a 0 (ej: 0.1), cambio a Moving
        if (Mathf.Abs(horizontalAxis) > Mathf.Epsilon && IsGrounded())
        {

            curState = FSMState.Moving;
            return;
        }
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
        return Physics2D.OverlapCircle(groundCheck.position, 0.12f, groundLayer) != null;
    }

    private float GetSlopeAngle()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.5f, groundLayer);
        if (hit.collider != null)
        {
            Vector2 surfaceNormal = hit.normal;
            Vector2 upVector = Vector2.up;

            float slopeAngle = Mathf.DeltaAngle(Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg, Mathf.Atan2(upVector.y, upVector.x) * Mathf.Rad2Deg);

            return slopeAngle;
        }

        return Vector2.Angle(hit.normal, Vector2.up);
    }

    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
    }
}
