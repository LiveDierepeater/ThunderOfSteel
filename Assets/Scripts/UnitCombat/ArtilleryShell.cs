using UnityEngine;

public class ArtilleryShell : Projectile
{
    public float MinArcHeight = 3f;
    public float MaxArcHeight = 50f;
    
    private Vector3 _initialPosition;
    private Vector3 _velocity;
    private float _launchTime;
    private const float _GroundLevel = 0f;
    private const float _Gravity = 9.81f;

    protected void Start()
    {
        _initialPosition = transform.position;
        _launchTime = Time.time;
        CalculateInitialVelocity();
    }
    
    protected override void Update()
    {
        base.Update();
        UpdateProjectilePosition();
    }

    protected override void UpdateProjectilePosition()
    {
        float timeSinceLaunch = Time.time - _launchTime;
        // Calculate current Position
        Vector3 position = _initialPosition + _velocity * timeSinceLaunch + Vector3.up * (0.5f * (-_Gravity * timeSinceLaunch * timeSinceLaunch));
        
        // Destroys Projectile when ground is hit
        if (position.y < _GroundLevel)
        {
            position.y = _GroundLevel;
            Destroy(gameObject);
        }
        else
        {
            transform.position = position;
        }
    }
    
    private void CalculateInitialVelocity()
    {
        Vector3 displacement = Target.transform.position - _initialPosition;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = displacementXZ.magnitude;
        float maxHeight = CalculateMaxHeightBasedOnDistance(horizontalDistance, MaxArcHeight, MinArcHeight, MaxArcHeight * 2);

        // Calculate flight time based on maximum altitude (time to reach maximum altitude, then double for return)
        float timeToApex = Mathf.Sqrt(2 * maxHeight / _Gravity);
        float totalFlightTime = 2 * timeToApex; // Total flight time is round trip to the Apex

        float initialVerticalVelocity = _Gravity * timeToApex; // Speed needed to reach the apex
        float initialHorizontalSpeed = horizontalDistance / totalFlightTime; // Horizontal speed adjustment

        // Set the initial speed
        _velocity = new Vector3(displacementXZ.normalized.x * initialHorizontalSpeed, initialVerticalVelocity, displacementXZ.normalized.z * initialHorizontalSpeed);
    }
    
    private float CalculateMaxHeightBasedOnDistance(float distance, float maxPossibleHeight, float minHeight, float minDistanceForMaxHeight)
    {
        return distance < minDistanceForMaxHeight ? Mathf.Lerp(minHeight, maxPossibleHeight, distance / minDistanceForMaxHeight) : maxPossibleHeight;
    }
}
