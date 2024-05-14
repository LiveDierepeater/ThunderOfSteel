using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float initialSpeed;
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
        Destroy(gameObject);
    }

    private void UpdateTargetPosition()
    {
        targetPosition = target.transform.position;
        // try
        // {
        //     targetPosition = target.transform.position;
        // }
        // catch (MissingReferenceException _)
        // {
        //     
        // }
    }

    protected virtual void UpdateProjectilePosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, initialSpeed * Time.deltaTime);
    }

    private bool DoesProjectileReachTargetPosition()
    {
        return Vector3.Distance(transform.position, targetPosition) < 0.1f;
    }
}
