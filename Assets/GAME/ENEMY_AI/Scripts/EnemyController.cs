using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyHealth enemyHealth;
    void Start()
    {
        
    }

    void Update()
    {

    }
    
    public void RecieveHit(float damage)
    {
        Debug.Log("Hit by player");
        enemyHealth.TakeDamage(damage);
    }
}
