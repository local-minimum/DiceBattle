using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void HealthChangeEvent(int newHealth, int delta);

public class PlayerCharacter : MonoBehaviour
{
    public static event HealthChangeEvent OnHealthChange;

    [SerializeField]
    ChangeableStatUI _healthUI;

    [SerializeField]
    ChangeableStatUI _defenceUI;

    [SerializeField]
    ActionCardGroup cardGroup;

    bool showDeath;
    [SerializeField]
    float showDeathDelay = 0.5f;
    float showDeathTime;

    public int Health {
        get => GameProgress.Health;
        set
        {
            GameProgress.Health = value;
            _healthUI.Value = GameProgress.Health;

            if (GameProgress.Health == 0)
            {
                showDeathTime = showDeathDelay + Time.timeSinceLevelLoad;
                showDeath = true;
            }
        }
    }

    public int Defence => GameProgress.BaseDefence + cardGroup.Defence;

    private void Start()
    {
        _defenceUI.SetValueWithoutChange(Defence);
        _healthUI.SetValueWithoutChange(GameProgress.Health);
    }

    private void OnEnable()
    {
        Monster.OnAttack += Monster_OnAttack;
        Battle.OnChangePhase += Battle_OnChangePhase;
        DieDropZone.OnChange += DieDropZone_OnChange;
    }


    private void OnDisable()
    {
        Monster.OnAttack -= Monster_OnAttack;
        Battle.OnChangePhase -= Battle_OnChangePhase;
        DieDropZone.OnChange -= DieDropZone_OnChange;
    }

    private void DieDropZone_OnChange(DieDropZone dropZone)
    {
        Debug.Log($"New defence is {Defence}");
        _defenceUI.Value = Defence;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        if (phase == BattlePhase.SelectNumberOfDice)
        {
            _defenceUI.Value = Defence;
        }
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

        if (showDeath && Time.timeSinceLevelLoad > showDeathTime)
        {
            GameProgress.InvokeGameOver();
        }
    }
}
