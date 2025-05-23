using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    private readonly Dictionary<int, Weaponry> _coolDowns = new Dictionary<int, Weaponry>();

    public static CooldownManager Instance { get; private set; }

    private readonly Queue<int> _keysToRemove = new Queue<int>();

    public TMP_Text _text;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void FixedUpdate()
    {
        foreach (var key in _coolDowns.Keys)
        {
            _coolDowns[key].RemoveCooldownTime(Time.deltaTime);
            
            if (_coolDowns[key].CurrentCoolDownTime <= 0)
            {
                // Cooldown expired
                _keysToRemove.Enqueue(key);
                
                // Here notification logic could be inserted, e.g. about an event.
            }
        }

        while (_keysToRemove.Count > 0)
        {
            _coolDowns.Remove(_keysToRemove.Dequeue());
        }

        //_text.text = _coolDowns.Count.ToString();
    }

    public void StartCooldown(int identifier, Weaponry weaponry)
    {
        _coolDowns.TryAdd(identifier, weaponry);
    }

    // Method to check cooldown status
    public bool IsCooldownActive(int identifier)
    {
        return _coolDowns.ContainsKey(identifier) && _coolDowns[identifier].CurrentCoolDownTime > 0;
    }
    
    // Additional methods such as checking if a cooldown is active could be added here.
}
