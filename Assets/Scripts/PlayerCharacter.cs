using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void HealthChangeEvent(int newHealth, int delta);

public class PlayerCharacter : MonoBehaviour
{
    public static event HealthChangeEvent OnHealthChange;

    [SerializeField]
    int startHealth = 42;

    [SerializeField]
    float showDeltaTime = 1;

    [SerializeField]
    TMPro.TextMeshProUGUI HealthText;

    int _health;
    bool showingDelta = false;
    float showHealthTime;

    public int Health {
        get => _health;
        set
        {
            int newHealth = Mathf.Max(0, value);
            int delta = newHealth - _health;
            _health = newHealth;
            HealthText.text = delta.ToString();
            showHealthTime = showDeltaTime + Time.timeSinceLevelLoad;
            showingDelta = true;
        }
    }

    public int Defence => 0;

    private void OnEnable()
    {
        Health = startHealth;
        Monster.OnAttack += Monster_OnAttack;
    }

    private void OnDisable()
    {
        Monster.OnAttack -= Monster_OnAttack;
    }
    
    private struct HealthChange
    {
        public int Health;
        public int Delta;

        public HealthChange(int health, int delta)
        {
            Health = health;
            Delta = delta;
        }
    }

    List<HealthChange> healthChanges = new List<HealthChange>();

    private void Monster_OnAttack(Monster monster, MonsterAttack attack)
    {
        var dmg = Mathf.Min(attack.Attack - Defence, Health);
        if (dmg > 0)
        {
            Health -= dmg;
            healthChanges.Add(new HealthChange(Health, -dmg));
        }
    }


    private void Update()
    {
        if (showingDelta && Time.timeSinceLevelLoad > showHealthTime)
        {
            showingDelta = false;
            HealthText.text = _health.ToString();
        }

        foreach (var change in healthChanges)
        {
            OnHealthChange?.Invoke(change.Health, change.Delta);
        }
        healthChanges.Clear();
    }
}
