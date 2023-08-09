using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Utils;

public delegate void MonstersWipeEvent();

public class MonsterManager : MonoBehaviour
{
    public static event MonstersWipeEvent OnWipe;

    [SerializeField]
    MonsterSettings[] MonsterSettings;

    List<Monster> Monsters = new List<Monster>();

    [SerializeField]
    Monster MonsterPrefab;

    [SerializeField, Range(0, 10)]
    int DifficultyCostPerMonster = 4;

    bool AnyAlive
    {
        get
        {
            return Monsters.Any(m => m.Alive);
        }
    }

    Monster GetMonster(int index)
    {
        if (index < Monsters.Count) return Monsters[0];

        var monster = Instantiate(MonsterPrefab, transform);
        Monsters.Add(monster);

        return monster;
    }

    private void Awake()
    {
        Monsters.Clear();
        Monsters.AddRange(GetComponentsInChildren<Monster>());
        Monsters[0].Configure(MonsterSettings[0]);
    }

    bool GetRandomMonster(int availableScore, out MonsterSettings monsterSettings)
    {
        var options = this.MonsterSettings.Where(m => m.DifficultyScore <= availableScore).Shuffle().ToArray();
        if (options.Length == 0)
        {
            monsterSettings = null;
            return false;
        }

        monsterSettings = options[0];
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
        var options = Monsters.Where(m => m.CanDoAction).ToArray();
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
