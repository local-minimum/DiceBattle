using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void MonsterReportEvent(Monster monster, string report);
public delegate void MonsterAttackEvent(Monster monster, MonsterAction attack);

public class Monster : MonoBehaviour
{
    public static event MonsterReportEvent OnReport;
    public static event MonsterAttackEvent OnAttack;

    public static Monster HoveredMonster { get; set; }

    [SerializeField]
    int _actionPointsPerTurn = 3;

    [SerializeField]
    TMPro.TextMeshProUGUI ActionPointsUI;

    int _actionPoints;

    public int ActionPoints {
        get => _actionPoints;
        set
        {
            _actionPoints = value;
            ActionPointsUI.text = $"AP: {value}";
        }
    }

    [SerializeField]
    ChangeableStatUI _health;

    [SerializeField]
    TMPro.TextMeshProUGUI NameText;

    [SerializeField]
    TMPro.TextMeshProUGUI StatusText;

    [SerializeField]
    int startHealth;

    [SerializeField]
    GameObject BaseCard;

    public int Health { 
        get => _health.Value; 
        set
        {
            _health.Value = value;

            if (_health.Value == 0)
            {
                GameProgress.XP += XpReward;
                deathHide = Time.timeSinceLevelLoad + DelayDeathHide;
            }
        }
    }

    [SerializeField]
    float DelayDeathHide = 0.5f;

    float deathHide;

    [SerializeField]
    TMPro.TextMeshProUGUI DefenceText;

    [SerializeField]
    int BaseDefence;

    public int Defence
    {
        get => BaseDefence + (ActionPoints > 0 ? actions.Where(a => a.IsDefence && !a.IsOnCooldown && a.ActionPoints <= ActionPoints).Sum(a => a.Value) : 0);
    }

    [SerializeField]
    int xpReward;

    [SerializeField]
    int startDice;

    [SerializeField]
    int minDicePerTurn = 1;

    [SerializeField]
    int maxDicePerTurn = 3;

    [SerializeField]
    AnimationCurve extraDiceProb;

    [SerializeField]
    MonsterActionSetting[] actionSettings;

    [SerializeField]
    MonsterAction actionPrefab;

    [SerializeField]
    RectTransform actionsRoot;

    [SerializeField]
    MonsterActionPreviewUI actionPreviewPrefab;

    [SerializeField]
    RectTransform actionsPreviewsRoot;

    [SerializeField]
    MonsterActionsManager actionsManager;

    List<MonsterActionPreviewUI> actionPreviews = new List<MonsterActionPreviewUI>();

    List<MonsterAction> actions = new List<MonsterAction>();

    MonsterAction GetAction(int index)
    {
        if (index < actions.Count) return actions[index];

        var action = Instantiate(actionPrefab, actionsRoot);

        actions.Add(action);

        return action;
    }

    public int XpReward => xpReward;

    public string Name => NameText.text;

    public bool Alive => Health > 0;

    private void Awake()
    {
        actions.AddRange(actionsRoot.GetComponentsInChildren<MonsterAction>());
        actionPreviews.AddRange(actionsPreviewsRoot.GetComponentsInChildren<MonsterActionPreviewUI>());        
    }

    private void OnEnable()
    {
        diceHeld = startDice;

        int idx = 0;
        for (; idx<actionSettings.Length; idx++)
        {
            var action = GetAction(idx);

            action.Config(actionSettings[idx]);
            action.gameObject.SetActive(false);
        }

        for (var l = actions.Count; idx<l; idx++)
        {
            actions[idx].gameObject.SetActive(false);
        }

        _health.SetValueWithoutChange(startHealth);

        Battle.OnChangePhase += Battle_OnChangePhase;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        if (!Alive) return;

        switch (phase)
        {
            case BattlePhase.Cleanup:
                Cleanup();
                break;
            case BattlePhase.SelectNumberOfDice:
                SelectNumberAndRollDice();
                break;
            case BattlePhase.RollDice:
                RollDice();
                break;
            case BattlePhase.UseDice:
                SlotDice();
                break;
        }
    }

    int diceHeld;
    int[] diceValues;

    public bool CanDoAction => Alive && ActionPoints > 0 && actions.Any(a => a.CanBeUsed && a.ActionPoints <= ActionPoints);

    public void DoAction()
    {
        var action = actions.Where(a => a.CanBeUsed && a.ActionPoints <= ActionPoints).OrderByDescending(a => ((float) a.Value) / a.ActionPoints).FirstOrDefault();
        if (action == null)
        {
            OnReport?.Invoke(this, "Can not attack");
            return;
        }

        if (action.IsAttack)
        {
            OnAttack?.Invoke(this, action);
        } else if (action.IsHeal)
        {
            OnReport?.Invoke(this, $"Uses {action.Name} with value {action.Value} to heal themselves");
            Health += action.Value;
        }

        ActionPoints -= action.ActionPoints;
        actionsManager.ShowAction(action);
        action.Use();

        SyncActionPreviews();
    }

    void SlotDice()
    {
        var sortedDice = diceValues.OrderByDescending(v => v).ToArray();
        var sortedActions = actions.OrderBy(a => !a.CanBeUsed).ThenBy(a => a.Cooldown).ToArray();

        for (int i = 0; i<sortedDice.Length; i++)
        {
            bool usedDie = false;
            var value = sortedDice[i];
            for (int j = 0, l=sortedActions.Length; j<l; j++)
            {
                var action = sortedActions[j];
                if (!action.CanBeUsed) continue;

                if (action.TakeDie(value))
                {
                    usedDie = true;
                    break;
                }
            }

            if (usedDie) continue;

            for (int j = 0, l=sortedActions.Length; j<l; j++)
            {
                var action = sortedActions[j];
                if (action.CanBeUsed) continue;

                if (action.TakeDie(value))
                {
                    break;
                }
            }

        }

        StatusText.text = "Slotted dice / waiting on turn";

        SyncActionPreviews();

        DefenceText.text = Defence.ToString();
    }

    public int ConsumeDefenceForAttack(int attack)
    {
        // TODO: This can be smarter so that they don't consume best defence first when not needed
        var defences = actions
            .Where(a => a.IsDefence && !a.IsOnCooldown && a.ActionPoints < ActionPoints)
            .OrderByDescending(a => ((float)a.Value) / a.ActionPoints)
            .ToArray();

        int defence = BaseDefence;

        for (int i = 0; i<defences.Length; i++)
        {
            var action = defences[i];

            if (action.ActionPoints <= ActionPoints)
            {
                defence += action.Value;
                ActionPoints -= action.ActionPoints;
                actionsManager.ShowAction(action);
                action.Use();
            }

            if (defence >= attack) break;
        }

        if (defence > 0)
        {
            DefenceText.text = Defence.ToString();
        }

        return defence;
    }

    void RollDice()
    {
        diceValues = diceValues.Select(_ => Random.Range(1, 7)).ToArray();

        var text = "";
        if (diceValues.Length == 0)
        {
            text = "No dice to slot"; ;
        } else if (diceValues.Length == 1)
        {
            text = $"Got die: {diceValues[0]}";
        } else
        {
            var diceText = string.Join(" ", diceValues);
            text = $"Got dice: {diceText}";
        }

        StatusText.text = text;
        OnReport?.Invoke(this, text);
    }

    void SelectNumberAndRollDice()
    {
        var diceCount = Mathf.Min(actions.Where(a => a.CanBeUsed).Sum(a => a.SuggestDiceThrowCount()), diceHeld);
        var t = 1f - ((float)diceHeld - diceCount) / startDice;

        if (diceCount < diceHeld && Random.value < extraDiceProb.Evaluate(t))
        {
            Debug.Log("Extra dice");
            diceCount++;
        }

        diceCount = Mathf.Min(diceHeld, Mathf.Clamp(diceCount, minDicePerTurn, maxDicePerTurn));
        diceHeld -= diceCount;
        diceValues = new int[diceCount];

        var text = $"Will roll {diceValues.Length} dice";
        StatusText.text = text;
        OnReport?.Invoke(this, text);
    }

    void Cleanup()
    {
        for (int i = 0,l=actions.Count; i<l; i++)
        {
            var action = actions[i];

            action.DecayDice();
            action.NewTurn();
        }
        SyncActionPreviews();
        ActionPoints = _actionPointsPerTurn;

        DefenceText.text = Defence.ToString();
    }

    MonsterActionPreviewUI ActionPreview(int index)
    {
        if (index < actionPreviews.Count) return actionPreviews[index];

        var preview = Instantiate(actionPreviewPrefab, actionsPreviewsRoot);
        actionPreviews.Add(preview);

        return preview;
    }

    void SyncActionPreviews()
    {
        var sorted = actions
            .OrderBy(a => a.IsOnCooldown)
            .ThenBy(a => a.IsOnCooldown || a.ActionPoints > ActionPoints ? a.Cooldown : -a.Value)
            .ToArray();

        int idx = 0;
        for (; idx<sorted.Length; idx++)
        {
            var preview = ActionPreview(idx);
            preview.Sync(sorted[idx], ActionPoints);
            preview.gameObject.SetActive(true);
        }

        for (var l = actionPreviews.Count; idx<l; idx++)
        {
            actionPreviews[idx].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Alive && BaseCard.activeSelf && Time.timeSinceLevelLoad > deathHide)
        {
            BaseCard.SetActive(false);
        }
    }
}
