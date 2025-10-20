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
    public float jumpForce = 10f;
    public bool isGrounded;
    public float gravity = 10f;

    [Header("Combat")]
    public bool isAttacking;

    [Header("References")]
    private Rigidbody2D rb;
    private Animator anim;

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
        GetInput();
        AnimationControl();
    }

    void FixedUpdate()
    {
        Move();
    }

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

        if (Input.GetKeyDown(KeyCode.Z) && isGrounded)  // Jump
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.C) && isGrounded)  // Sprint
        {
            Sprint();
        }
        else if (isGrounded)
        {
            StopSprinting();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Attack();
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
        rb.linearVelocity = new Vector2(moveX * currentSpeed, jumpForce);
        anim.SetTrigger("Jumping");
    }
    #endregion

    #region Combat
    private void Attack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack_1");
    }
    public void EndAttack()
    {
        isAttacking = false;
    }
    #endregion

    #region Ground Check
    private void OnTriggerEnter2D(Collider2D other) // vertical collision, feet touching ground
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isGrounded = true;
            anim.SetBool("Grounded", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isGrounded = false;
            anim.SetBool("Grounded", false);
        }
    }
    #endregion

    private void AnimationControl()
    {
        anim.SetFloat("MoveMagnitude", moveMagnitude); // move
    }
}