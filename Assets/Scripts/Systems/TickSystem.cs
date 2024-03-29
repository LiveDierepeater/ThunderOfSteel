using UnityEngine;

public class TickSystem : MonoBehaviour
{
    public delegate void TickDelegate();
    public TickDelegate OnTick;

    public const float TickRate = 0.2f;
    private float nextTick;

    private void Awake()
    {
        TickManager.Instance.TickSystem = this;
    }

    void Update()
    {
        TickTimer();
    }

    private void TickTimer()
    {
        if (Time.time >= nextTick)
        {
            // Calls Tick-Event here.
            OnTick?.Invoke();
            
            nextTick = Time.time + TickRate;
        }
    }
}
