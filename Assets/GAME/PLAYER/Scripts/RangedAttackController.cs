using UnityEngine;

public class RangedAttackController : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    
    public void ShootProjectile()
    {
        Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
    }
}
