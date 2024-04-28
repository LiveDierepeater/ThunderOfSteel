using UnityEngine;

public class ArtilleryShell : Projectile
{
    public float arcHeight = 10.0f;

    private Vector3 startPosition;

    protected void Start()
    {
        startPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();
        ParabolicTrajectory();
    }

    private void ParabolicTrajectory()
    {
        float dist = Vector3.Distance(startPosition, targetPosition);
        float arcRatio = Mathf.Clamp01((transform.position - startPosition).magnitude / dist);
        float height = Mathf.Sin(arcRatio * Mathf.PI) * arcHeight;
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
    }
}
