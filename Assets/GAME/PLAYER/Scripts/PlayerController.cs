using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Timeline;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private float currentSpeed;
    private float moveX;
    public float facingDirection;
    private float moveMagnitude;
    private bool jumpRequested;
    public float jumpForce = 10f;
    public float gravity = 10f;
    public bool isWallSliding;
    public float wallSlideSpeed = 2f;

    [Header("Combat")]
    public bool isAttacking;
    public float comboWindowDuration = 0.6f;
    private float comboTimer = 0f;
    public int maxCombo = 3;
    private int comboIndex = 0;

    [Header("Ground Check")]
    public Vector2 boxSize;
    public float castDistance;

    [Header("References")]
    private Rigidbody2D rb;
    private Animator anim;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform wallCheck;
    public Transform shootPoint;

    [Header("Script References")]
    public MeleeAttackController meleeAttackController;
    public RangedAttackController rangedAttackController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        anim = GetComponent<Animator>();
        
        currentSpeed = moveSpeed;
        facingDirection = 1;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();

        GetInput();
        AnimationControl();
    }

    void FixedUpdate()
    {
        Move();
        Jump();
        WallSlide();
    }

    #region Timers
    private void UpdateTimers()
    {
        //if (attackCooldownTimer > 0f)
        // {
        //     attackCooldownTimer -= Time.deltaTime;
        //     if (attackCooldownTimer < 0f) attackCooldownTimer = 0f;
        // }

        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                comboTimer = 0f;
                comboIndex = 0; // kombo penceresi bittiğinde kombo sıfırlansın
            }
        }
    }
    #endregion

    #region Input
    private void GetInput()
    {
        if (!isAttacking)
        {
            moveX = Input.GetAxisRaw("Horizontal"); // Move
            if (moveX != 0) facingDirection = Mathf.Sign(moveX);
        }
        moveMagnitude = Mathf.Abs(moveX);

        if (facingDirection < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (facingDirection > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (Input.GetKeyDown(KeyCode.Z) && IsGrounded() && !isAttacking)  // Jump
        {
            jumpRequested = true;
        }

        if (Input.GetKey(KeyCode.C) && IsGrounded() && Mathf.Abs(moveX) > 0.05f) // Sprint
        {
            Sprint();
        }
        else if (IsGrounded())
        {
            StopSprinting();
        }

        if (Input.GetKeyDown(KeyCode.X) && !isAttacking) // Attack
        {
            if (!IsGrounded())
            {
                JumpAttack();
            }
            else
            {
                ComboAttack();
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && !isAttacking) // Ranged Attack
        {
            RangedAttack();
        }
    }
    #endregion

    #region Movement
    private void Move()
    {
        if (isAttacking) return;

        rb.linearVelocity = new Vector2(moveX * currentSpeed, rb.linearVelocity.y);
    }
    private void Sprint()
    {
        currentSpeed = moveSpeed * 1.5f;
        anim.SetBool("Sprinting", true);
    }
    private void StopSprinting()
    {
        currentSpeed = moveSpeed;
        anim.SetBool("Sprinting", false);
    }
    private void Jump()
    {
        if (jumpRequested && IsGrounded())
        {
            rb.linearVelocity = new Vector2(moveX * currentSpeed, jumpForce);
            anim.SetTrigger("Jumping");
            jumpRequested = false;
        }
    }

    private bool Walled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    private void WallSlide()
    {
        if (Walled() && !IsGrounded() && moveMagnitude != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(0, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    #endregion

    #region Combat
    private void ComboAttack()
    {
        isAttacking = true;

        currentSpeed = 0f;

        int nextCombo = comboIndex + 1;
        if (nextCombo > maxCombo) nextCombo = 1;

        anim.SetTrigger("Attack_" + nextCombo);

        comboTimer = comboWindowDuration;

        comboIndex = nextCombo;
    }

    private void JumpAttack()
    {
        isAttacking = true;
        currentSpeed = 0f;

        comboIndex = 0;
        comboTimer = 0f;

        anim.SetTrigger("JumpAttack");
    }

    private void RangedAttack()
    {
        isAttacking = true;
        currentSpeed = 0f;

        anim.SetTrigger("RangedAttack");
    }

    public void EndAttack()
    {
        isAttacking = false;
        currentSpeed = moveSpeed;
    }
    #endregion

    #region Ground Check
    public bool IsGrounded()
    {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer) || Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, wallLayer))
        {
            anim.SetBool("Grounded", true);
            return true;
        }
        else
        {
            anim.SetBool("Grounded", false);
            return false;
        }
    }
    #endregion

    private void AnimationControl()
    {
        anim.SetFloat("MoveMagnitude", moveMagnitude); // move
    }

    #region Forwarded Animation Events
    public void EnableMeleeHitbox()
    {
        meleeAttackController.EnableHitbox();
    }
    public void DisableMeleeHitbox()
    {
        meleeAttackController.DisableHitbox();
    }

    public void RangedAttackProjectile()
    {
        rangedAttackController.ShootProjectile();
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }
}
