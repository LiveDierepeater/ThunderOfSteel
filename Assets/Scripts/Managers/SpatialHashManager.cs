using UnityEngine;

public class SpatialHashManager : MonoBehaviour
{
    public static SpatialHashManager Instance { get; private set; }
    public readonly SpatialHash SpatialHash = new SpatialHash();

    // DEBUG
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int RegisteredUnits { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // DEBUG
    private void Update()
    {
        RegisteredUnits = SpatialHash.RegisteredUnits;
    }
}
