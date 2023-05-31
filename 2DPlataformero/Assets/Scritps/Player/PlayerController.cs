using Cinemachine;
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : FSM
{
    public enum FSMState
    {
        None, Idle, Moving, Jump, Fall, Attack, Hit, Dead
    }

    public FSMState curState = FSMState.None;

    [Header("Referencias")]

    [SerializeField] private Animator animator;
    [SerializeField] private CharacterData characterData;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform characterSprite; // Referencia al objeto vacío que contiene el sprite del personaje
    [SerializeField] private Transform attackPosOrigin;
    [SerializeField] private TextMeshPro lifeUI;
    [SerializeField] private PlayerInput inputActions;
    private InputActionMap actionMap;
    private Rigidbody2D rb;

    [Header("GroundChecks")]

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float slideSpeed = 2.0f;

    [SyncVar(hook = nameof(UpdateHealthClient))]
    private float health;

    //Input
    private float horizontalAxis;
    private bool jump = false;
    private bool attack = false;
    private bool isPlayerDead = false;

    #region Network Methods
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        virtualCamera.Follow = transform;
        rb = GetComponent<Rigidbody2D>();
        inputActions.enabled = true;

        // Acceder al ActionMap asignado
        actionMap = inputActions.currentActionMap;
    }
    #endregion

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

        horizontalAxis = actionMap["Move"].ReadValue<float>();
        jump = actionMap["Jump"].triggered;
        attack = actionMap["Attack"].triggered;

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

    #region States
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
            if (rb.isKinematic == true)
            {
                rb.isKinematic = false;
            }

            curState = FSMState.Jump;
            return;
        }

        if (attack)
        {
            if (rb.isKinematic == true)
            {
                rb.isKinematic = false;
            }

            curState = FSMState.Attack;
            return;
        }

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

        //Movimiento basado en Velocidad
        if (IsGrounded())
        {
            //Si estoy en el suelo y el angulo de la rampa es menor que el maximo permitido, ajustar gravedad a 5
            if (Mathf.Abs(slopeAngle) < characterData.maxSlopeAngle && slopeAngle != 0)
            {
                rb.gravityScale = 10;
            }
            else
            {
                rb.gravityScale = 2.5f;
            }

            rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);

        }

        // Reglas de Transición

        if (attack)
        {
            rb.gravityScale = 2.5f;
            curState = FSMState.Attack;
            return;
        }

        if (jump && IsGrounded())
        {
            rb.gravityScale = 2.5f;
            curState = FSMState.Jump;
            return;
        }

        if (!IsGrounded() || Mathf.Abs(slopeAngle) > characterData.maxSlopeAngle)
        {
            rb.gravityScale = 2.5f;
            curState = FSMState.Fall;
            return;

        }

        if (Mathf.Abs(horizontalAxis) < Mathf.Epsilon && IsGrounded())
        {
            rb.gravityScale = 2.5f;
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

        if (attack)
        {
            curState = FSMState.Attack;
            return;
        }

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

        if (attack)
        {
            curState = FSMState.Attack;
            return;
        }

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

        float slopeAngle = GetSlopeAngle();

        rb.velocity = new Vector2(horizontalAxis * characterData.moveSpeed, rb.velocity.y);

        //Meter dentro de este IF todo el Estado (CUANDO TENGA ANIMACIONES)
        //
        //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttackAnimation"))

        //animator.Play("PlayerAttackAnimation");

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPosOrigin.position, characterData.attackRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider2D enemy in enemies)
        {
            //Daño a jugadores
            //Primero Obtenemos el Script
            PlayerController playerController = enemy.GetComponent<PlayerController>();

            //Si el playercontroller no somos nosotros
            if (playerController.netId != this.netId)
            {
                //Llamamos en el servidor a un CMD que recibe el Target y un daño (MODIFICAR A FUTURO POR EL DAÑO TOTAL)
                CmdHitEnemy(playerController.gameObject, characterData.attack);
            }
        }

        //Reglas de Transición

        //Meter dentro de este IF las transiciones (SOLO CUANDO TENGA ANIMACIONES)
        //
        //if (animator.GetAnimatorTransitionInfo(0).normalizedTime > 0.9f)

        if (Mathf.Abs(horizontalAxis) < Mathf.Epsilon && IsGrounded())
        {
            curState = FSMState.Idle;
            return;
        }

        if (Mathf.Abs(horizontalAxis) > Mathf.Epsilon)
        {
            curState = FSMState.Moving;
            return;
        }

        if (!IsGrounded() || Mathf.Abs(slopeAngle) > characterData.maxSlopeAngle)
        {
            curState = FSMState.Fall;
            return;
        }
    }

    private void UpdateHitState()
    {
        // Actualizar Estado

        float slopeAngle = GetSlopeAngle();

        // animator.Play("CharacterHitAnimation");
        // Añadir fuerza al hit

        // Reglas de Transición

        if (Mathf.Abs(horizontalAxis) < Mathf.Epsilon && IsGrounded())
        {
            curState = FSMState.Idle;
        }

        if (!IsGrounded() || Mathf.Abs(slopeAngle) > characterData.maxSlopeAngle)
        {
            curState = FSMState.Fall;
        }
    }

    private void UpdateDeadState()
    {
        // Actualizar Estado

        // animator.Play("CharacterDeadAnimation");

        isPlayerDead = true;

        // Reglas de Transición
    }

    #endregion

    #region Reusable Methods
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

    public void SetCharacterData(CharacterData selectedCharacter)
    {
        characterData = selectedCharacter;
        health = characterData.health;
        lifeUI.text = health.ToString() + "PV";
        CmdRequestPlayerHealthUpdates();
    }

    #endregion

    #region InputEnable&Disable

    public void OnEnableMovement()
    {
        if (!isLocalPlayer) { return; }

        actionMap["AutoDamage"].performed += OnAutoDamagePerformed;
        inputActions.enabled = true;
    }

    public void OnDisableMovement()
    {
        if (!isLocalPlayer) { return; }

        actionMap["AutoDamage"].performed -= OnAutoDamagePerformed;
        inputActions.enabled = false;
    }

    //Test
    private void OnAutoDamagePerformed(InputAction.CallbackContext autoDamage)
    {
        if (autoDamage.action.triggered)
        {
            CmdTakeDamage(1);
        }
    }
    #endregion

    #region HealthChange

    private void UpdateHealthClient(float oldHealth, float newHealth)
    {
        lifeUI.text = newHealth.ToString() + "PV";
    }

    //El servidor procesa la solicitud. No requiere autoridad poder actualizar la vida de los demás.
    [Command(requiresAuthority = false)]
    private void CmdRequestPlayerHealthUpdates()
    {
        RpcUpdateHealth(health);
    }

    //El Servidor propaga la vida y UI actuales a los jugadores
    [ClientRpc]
    private void RpcUpdateHealth(float newHealth)
    {
        // Actualizar la vida en los clientes
        health = newHealth;
        // Actualizar la interfaz de usuario (UI) con la nueva vida
        lifeUI.text = newHealth.ToString() + "PV";
    }
    #endregion

    #region TakeDamage

    //El servidor recibe el daño que el jugador se hace
    [Command]
    private void CmdTakeDamage(int damage)
    {
        health -= damage;

        //Esto es para que el número de vida nunca sea negativo
        if (health < 0)
        {
            health = 0;
        }

        // Llamar a CmdUpdateHealth para propagar la actualización a los clientes
        RpcUpdateHealth(health);
    }
    #endregion

    #region HitEnemy

    //Comando para golpear a un enemigo
    [Command]
    private void CmdHitEnemy(GameObject target, float damage)
    {
        PlayerController targetPlayer = target.GetComponent<PlayerController>();
        targetPlayer.health -= damage;

        if (targetPlayer.health < 0)
        {
            targetPlayer.health = 0;
        }

        // Llamar a RpcUpdateHealth en el jugador objetivo para propagar la actualización a los clientes
        targetPlayer.RpcUpdateHealth(targetPlayer.health);
    }
    #endregion
}
