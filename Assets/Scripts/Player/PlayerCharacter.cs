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
    ChangeableStatUI _health;

    public int Health {
        get => _health.Value;
        set
        {
            _health.Value = value;
        }
    }

    public int Defence => 0;

    private void OnEnable()
    {
        _health.SetValueWithoutChange(startHealth);
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

    private void Monster_OnAttack(Monster monster, MonsterAction action)
    {
        var dmg = Mathf.Min(action.Value - Defence, Health);
        if (dmg > 0)
        {
            Health -= dmg;
            healthChanges.Add(new HealthChange(Health, -dmg));
        }
    }


    private void Update()
    {
        foreach (var change in healthChanges)
        {
            OnHealthChange?.Invoke(change.Health, change.Delta);
        }
        healthChanges.Clear();
    }
}
