using System;
using UnityEngine;

public class SpatialHashManager : MonoBehaviour
{
    public static SpatialHashManager Instance { get; private set; }
    public readonly SpatialHash SpatialHash = new SpatialHash();

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

    private void Start()
    {
        SpatialHash.InitializeCellOffsetsForDistanceCalculation();
    }

    // Optional Methods for Adding/Removing of Objects could get added here as Wrapper.
}
