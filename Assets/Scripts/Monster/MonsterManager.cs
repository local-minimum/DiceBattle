using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Utils;

public delegate void MonstersWipeEvent();

public class MonsterManager : DeCrawl.Primitives.FindingSingleton<MonsterManager> 
{
    public static event MonstersWipeEvent OnWipe;

    [SerializeField]
    MonsterSettings[] MonsterSettings;

    List<Monster> Monsters = new List<Monster>();

    [SerializeField]
    Monster MonsterPrefab;

    [SerializeField]
    AnimationCurve probabilityByDifficulty;

    [SerializeField, Range(0, 20)]
    int DifficultyCostPerMonster = 4;

    IEnumerable<Monster> ActiveMonsters => Monsters.Where(m => m.gameObject.activeSelf);

    bool AnyAlive
    {
        get
        {
            return ActiveMonsters.Any(m => m.Alive);
        }
    }

    public bool CanHurtAnyMonster(ActionCard action) => action.Action == ActionType.Attack && ActiveMonsters.Any(m => m.Defence < action.Value);
    public bool CanAffectAnyMonster(ActionCard action) => 
        action.Action == ActionType.Attack && ActiveMonsters.Any(m => m.BaseDefence < action.Value);

    public Monster FirstAffectableMonster(ActionCard action) => 
        action.Action == ActionType.Attack ? ActiveMonsters.FirstOrDefault(m => m.BaseDefence < action.Value) : null;


    Monster GetMonster(int index)
    {
        if (index < Monsters.Count) return Monsters[0];

        var monster = Instantiate(MonsterPrefab, transform);
        Monsters.Add(monster);

        return monster;
    }

    bool GetRandomMonster(int availableScore, out MonsterSettings monsterSettings)
    {
        var options = this.MonsterSettings
            .Where(m => m.DifficultyScore <= availableScore)
            .OrderBy(m => m.DifficultyScore)
            .ToArray();
        if (options.Length == 0)
        {
            monsterSettings = null;
            return false;
        }

        var probabilities = options.Select(m => probabilityByDifficulty.Evaluate(((float)m.DifficultyScore) / availableScore)).ToArray();
        var value =  Random.Range(0f, probabilities.Sum());

        Debug.Log($"[Monster Manager] Selecting monster using {value} with probabilities {(string.Join(" | ", probabilities.Select((p, i) => $"{options[i].Name} {p}")))}");
        monsterSettings = options[options.Length - 1];

        for (int i = 0; i<probabilities.Length; i++)
        {
            if (value < probabilities[i])
            {
                monsterSettings = options[i];
                break;
            }
            value -= probabilities[i];
        }
        return true;
    }

    void ConfigureMonsters()
    {
        int fightDifficulty = GameProgress.Fights * GameSettings.MonsterDifficultyPerFight + GameSettings.MonsterDifficultyBase;
        Debug.Log($"[Monsters Manager] Fight Difficulty {fightDifficulty}");

        for (int i = 0; i<GameSettings.MaxMonstersInFight; i++)
        {
            var monster = GetMonster(i);
            if (GetRandomMonster(fightDifficulty, out var settings))
            {
                monster.Configure(settings);
                fightDifficulty -= settings.DifficultyScore;
                monster.gameObject.SetActive(true);

                fightDifficulty -= DifficultyCostPerMonster;

                Debug.Log($"[Monsters Manager] Adding [{settings.Name}] to fight");
            } else
            {
                monster.gameObject.SetActive(false);
            }

        }
    }

    private void Start()
    {
        Monsters.Clear();
        Monsters.AddRange(GetComponentsInChildren<Monster>(true));

        ConfigureMonsters();
    }

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
        Monster.OnDeath += Monster_OnDeath;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        Monster.OnDeath -= Monster_OnDeath;
    }

    private void Monster_OnDeath(Monster monster)
    {
        if (!AnyAlive)
        {
            GameProgress.IncreaseFights();

            OnWipe?.Invoke();
        }
    }

    bool mayTriggerMonsterAction;
    float nextMonsterAction;

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.MonsterAttack:
                nextMonsterAction = Time.timeSinceLevelLoad + GameSettings.MonsterPhasePreDelay;
                mayTriggerMonsterAction = true;
                break;
        }
    }

    bool SelectMonsterToDoAction()
    {
        var options = ActiveMonsters.Where(m => m.CanDoAction).ToArray();
        if (options.Length == 0)
        {
            return false;
        }

        options[Random.Range(0, options.Length)].DoAction();

        return true;
    }

    bool mayEndPhase;
    float endPhaseTime;

    private void Update()
    {
        if (mayTriggerMonsterAction && Time.timeSinceLevelLoad > nextMonsterAction)
        {
            if (SelectMonsterToDoAction())
            {
                nextMonsterAction = Time.timeSinceLevelLoad + GameSettings.MonsterPhaseAttackDuration;
            } else {
                mayTriggerMonsterAction = false;
                endPhaseTime = Time.timeSinceLevelLoad + GameSettings.MonsterPhasePostDelay;
                mayEndPhase = true;
            }
        } else if (mayEndPhase && Time.timeSinceLevelLoad > endPhaseTime)
        {
            mayEndPhase = false;
            Battle.Phase = Battle.Phase.NextPhase();
        }
    }
}
