using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    private float direction;
    private Rigidbody2D rb;
    private PlayerController playerController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = FindFirstObjectByType<PlayerController>();
        direction = playerController.facingDirection;
        if (direction < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(speed * direction, rb.linearVelocity.y);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}
