using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float speed;
    public Unit target;
    public Vector3 targetPosition;

    protected virtual void Update()
    {
        UpdateTargetPosition();
        UpdateProjectilePosition();    

        if (DoesProjectileReachTargetPosition())
            HitTarget();
    }

    protected virtual void HitTarget()
    {
        // Füge dem Ziel Schaden zu
        if (target is not null)
            print("Hit!");
            //target.ReceiveDamage(damage);
        Destroy(gameObject);  // Zerstöre das Projektil nach dem Treffer
    }

    private void UpdateTargetPosition()
    {
        if (target is not null)
            targetPosition = target.transform.position;
    }

    private void UpdateProjectilePosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private bool DoesProjectileReachTargetPosition()
    {
        return Vector3.Distance(transform.position, targetPosition) < 0.1f;
    }
}
