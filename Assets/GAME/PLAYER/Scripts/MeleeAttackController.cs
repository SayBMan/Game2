using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MeleeAttackController : MonoBehaviour
{
    public LayerMask enemyLayer;
    public EnemyController enemyController;
    public float damage;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    public void EnableHitbox()
    {
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ( ((1 << other.gameObject.layer) & enemyLayer.value) == 0 ) return;
        enemyController.RecieveHit(damage);
    }
}
