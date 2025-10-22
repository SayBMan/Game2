using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Timeline;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private float currentSpeed;
    public float moveX;
    private float moveMagnitude;
    private bool jumpRequested;
    public float jumpForce = 10f;
    public float gravity = 10f;
    public bool isWallSliding;
    public float wallSlideSpeed = 2f;

    [Header("Combat")]
    public bool isAttacking;
    //public float attackCooldownDuration = 0.18f;
    //private float attackCooldownTimer = 0f;
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        anim = GetComponent<Animator>();
        currentSpeed = moveSpeed;
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
        }
        moveMagnitude = Mathf.Abs(moveX);

        if (moveX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (moveX > 0)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }
}
