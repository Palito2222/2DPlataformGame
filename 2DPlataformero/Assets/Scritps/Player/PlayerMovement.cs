using Cinemachine;
using Mirror;
using UnityEngine;


public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private CharacterData characterData;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            return;
        }

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        virtualCamera.Follow = transform;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, characterData.jumpSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        float movementInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(movementInput * characterData.moveSpeed, rb.velocity.y);
    }

    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
    }
}

