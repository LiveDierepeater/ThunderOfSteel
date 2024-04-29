using UnityEngine;

public class ArtilleryShell : Projectile
{
    public AnimationCurve heightCurve;
    public float maxArcHeight = 50f;
    
    private Vector3 initialPosition;
    private float totalTime;
    private float elapsedTime;

    protected void Start()
    {
        initialPosition = transform.position;
        
        if (target is not null)
        {
            CalculateTrajectory();
        }
    }
    
    protected override void Update()
    {
        base.Update();
        UpdateProjectilePosition();
    }

    protected override void UpdateProjectilePosition()
    {
        if (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / totalTime; // Normalisierte Zeit für die Animationskurve

            // Berechnung der neuen Position
            Vector3 newPosition = Vector3.Lerp(initialPosition, new Vector3(target.transform.position.x, initialPosition.y, target.transform.position.z), normalizedTime);
            float heightFactor = heightCurve.Evaluate(normalizedTime); // Wert der Animationskurve an der aktuellen Zeitposition
            newPosition.y += heightFactor * maxArcHeight; // Anwendung der Höhenmodulation

            transform.position = newPosition;

            // Zerstören des Projektils, wenn es den Boden oder das Ziel erreicht
            if (newPosition.y < 0 || normalizedTime >= 1.0f)
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void CalculateTrajectory()
    {
        Vector3 targetPosition = target.transform.position;
        Vector3 displacement = targetPosition - transform.position;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = displacementXZ.magnitude;

        // Rufe die Methode auf, um die maximale Höhe basierend auf der Distanz zu bestimmen
        float dynamicMaxHeight = CalculateMaxHeightBasedOnDistance(horizontalDistance, maxArcHeight, 5f, 50f); // Beispielswerte

        totalTime = horizontalDistance / initialSpeed;
        float normalizedTime = elapsedTime / totalTime;
        float heightFactor = heightCurve.Evaluate(normalizedTime);
        newPosition.y += heightFactor * dynamicMaxHeight; // Verwende die dynamische Maximalhöhe
    }
    
    private float CalculateMaxHeightBasedOnDistance(float distance, float maxPossibleHeight, float minHeight, float minDistanceForMaxHeight)
    {
        if (distance < minDistanceForMaxHeight)
        {
            // Skaliere die maximale Höhe linear basierend auf der Distanz
            return Mathf.Lerp(minHeight, maxPossibleHeight, distance / minDistanceForMaxHeight);
        }
        return maxPossibleHeight; // Verwende die maximale Höhe, wenn der Mindestabstand erreicht oder überschritten wird
    }
}
