using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void MonsterReportEvent(Monster monster, string report);
public delegate void MonsterAttackEvent(Monster monster, MonsterAction attack);
public delegate void MonsterDeathEvent(Monster monster);

public class Monster : MonoBehaviour
{
    public static event MonsterReportEvent OnReport;
    public static event MonsterAttackEvent OnAttack;
    public static event MonsterDeathEvent OnDeath;

    public static Monster HoveredMonster { get; set; }

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
    GameObject BaseCard;

    public int Health { 
        get => _health.Value; 
        set
        {
            _health.Value = Mathf.Min(value, settings.MaxHealth);

            if (_health.Value == 0)
            {
                GameProgress.XP += XpReward;
                KillGate.Lock();
                OnDeath?.Invoke(this);
            }
        }
    }

    [SerializeField]
    DelayedGate KillGate = new DelayedGate();

    [SerializeField]
    TMPro.TextMeshProUGUI DefenceText;

    public int BaseDefence => settings.BaseDefence;
    public int Defence  => settings.BaseDefence + (ActionPoints > 0 ? actions.Where(a => a.IsDefence && !a.IsOnCooldown && a.ActionPoints <= ActionPoints).Sum(a => a.Value) : 0);

    [SerializeField]
    MonsterAction actionPrefab;

    [SerializeField]
    RectTransform actionsRoot;

    [SerializeField]
    MonsterActionPreviewUI actionPreviewPrefab;

    [SerializeField]
    RectTransform actionsPreviewsRoot;

    List<MonsterActionPreviewUI> actionPreviews = new List<MonsterActionPreviewUI>();

    List<MonsterAction> actions = new List<MonsterAction>();

    MonsterAction GetAction(int index)
    {
        if (index < actions.Count) return actions[index];

        var action = Instantiate(actionPrefab, actionsRoot);

        actions.Add(action);

        return action;
    }

    public int XpReward => settings.XPReward;

    public string Name => settings.Name;

    public bool Alive => Health > 0;

    MonsterSettings settings;

    void ConfigurateActions(MonsterActionSetting[] actionSettings)
    {
        int idx = 0;
        for (; idx<actionSettings.Length; idx++)
        {
            var action = GetAction(idx);

            action.Config(actionSettings[idx]);
            action.gameObject.SetActive(false);
            action.OnRestoreOrder += Action_OnRestoreOrder;
        }

        for (var l = actions.Count; idx<l; idx++)
        {
            actions[idx].gameObject.SetActive(false);
            actions[idx].OnRestoreOrder -= Action_OnRestoreOrder;
        }
    }

    private void Action_OnRestoreOrder()
    {
        ShowPossibleActions(false, null);
    }

    public void Configure(MonsterSettings settings)
    {
        this.settings = settings;
        NameText.text = settings.Name;
        
        _health.SetValueWithoutChange(settings.MaxHealth);

        diceHeld = settings.Dice;

        ConfigurateActions(settings.EquipActions().ToArray());
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        actions.Clear();
        actions.AddRange(actionsRoot.GetComponentsInChildren<MonsterAction>());
        actionPreviews.Clear();
        actionPreviews.AddRange(actionsPreviewsRoot.GetComponentsInChildren<MonsterActionPreviewUI>());        
    }

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
        ActionCard.OnStatus += ActionCard_OnStatus;
        MonsterAction.OnUse += MonsterAction_OnUse;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        ActionCard.OnStatus -= ActionCard_OnStatus;
        MonsterAction.OnUse -= MonsterAction_OnUse;
    }

    private void MonsterAction_OnUse(MonsterAction action)
    {
        if (actions.Contains(action)) ShowPossibleActions(false, action);
    }

    private void ActionCard_OnStatus(ActionCard card, ActionCardStatus status)
    {
        switch (status)
        {
            case ActionCardStatus.DragStart:
                HideAllActions();
                break;
            case ActionCardStatus.DragEnd:
                ShowPossibleActions(true, null);
                break;
        }
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        if (!Alive) return;

        switch (phase)
        {
            case BattlePhase.Cleanup:
                Cleanup();
                HideAllActions();
                break;
            case BattlePhase.SelectNumberOfDice:
                SelectNumberAndRollDice();
                ShowPossibleActions(true, null);
                break;
            case BattlePhase.RollDice:
                RollDice();
                break;
            case BattlePhase.UseDice:
                SlotDice();
                break;
            case BattlePhase.MonsterAttack:
                // RevealDiceValues();
                break;
        }
    }

    int diceHeld;
    int[] diceValues;

    public bool CanDoAction => Alive && ActionPoints > 0 && actions.Any(a => a.IsUsableActiveAction && a.ActionPoints <= ActionPoints);

    public void DoAction()
    {
        var action = actions
            .Where(a => a.IsUsableActiveAction && a.ActionPoints <= ActionPoints && a.Value > 0)
            .OrderByDescending(a => ((float) a.Value) / a.ActionPoints).FirstOrDefault();
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
        action.Use();

        SyncActionPreviews();
    }

    void SlotDice()
    {
        var sortedDice = diceValues.OrderByDescending(v => v).ToArray();
        var sortedActions = actions.OrderBy(a => !a.IsUsableActiveAction).ThenBy(a => a.Cooldown).ToArray();

        for (int i = 0; i<sortedDice.Length; i++)
        {
            bool usedDie = false;
            var value = sortedDice[i];
            for (int j = 0, l=sortedActions.Length; j<l; j++)
            {
                var action = sortedActions[j];
                if (action.IsOnCooldown) continue;

                if (action.TakeDie(value))
                {
                    Debug.Log($"[{Name}] <{action.Name}> took die with value {value} ({action.ValueRange} / {action.DiceDetails})");
                    usedDie = true;
                    break;
                }
            }

            if (usedDie) continue;

            for (int j = 0, l=sortedActions.Length; j<l; j++)
            {
                var action = sortedActions[j];
                if (!action.IsOnCooldown) continue;

                if (action.TakeDie(value))
                {
                    usedDie = true;
                    Debug.Log($"[{Name}] <{action.Name}> took die with value {value} ({action.ValueRange} / {action.DiceDetails})");
                    break;
                }
            }

            if (!usedDie)
            {
                Debug.LogWarning($"[{Name}] could not use a die with value {value}");
            }

        }

        StatusText.text = "Slotted dice / waiting on turn";

        SyncActionPreviews();
    }

    public int ConsumeDefenceForAttack(int attack)
    {
        if (attack <= 0)
        {
            Debug.Log($"[{Name}] Disregards attack since it has no value");
            return 0;
        }

        Debug.Log($"[{Name}] Preparing defence, having {ActionPoints} to spend");
        Debug.Log($"[{Name}] Defence without cooldown {(string.Join(" | ", actions.Where(a => a.IsDefence && !a.IsOnCooldown).Select(a => $"[{a.Name}: {a.Value}/{a.ActionPoints}]")))}");

        // TODO: This can be smarter so that they don't consume best defence first when not needed
        var defences = actions
            .Where(a => a.IsDefence && a.ActionPoints <= ActionPoints && a.IsUsablePassiveAction)
            .OrderByDescending(a => ((float)a.Value) / a.ActionPoints)
            .ToArray();

        Debug.Log($"[{Name}] Potential defences: {(string.Join("| ", defences.Select(d => d.Value)))}");

        int defence = settings.BaseDefence;

        for (int i = 0; i<defences.Length; i++)
        {
            if (defence >= attack) break;

            var action = defences[i];

            if (action.ActionPoints <= ActionPoints)
            {
                defence += action.Value;
                ActionPoints -= action.ActionPoints;
                action.Use();
            }

            if (defence >= attack) break;
        }

        Debug.Log($"[{Name}] Gathered a defence of {defence} ({BaseDefence})");

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

        Debug.Log($"[{Name}] {text}");
        StatusText.text = "";
    }

    void SelectNumberAndRollDice()
    {
        var diceCount = Mathf.Min(actions.Where(a => !a.IsOnCooldown).Sum(a => a.SuggestDiceThrowCount()), diceHeld);
        var t = 1f - ((float)diceHeld - diceCount) / settings.Dice;

        if (diceCount < diceHeld && Random.value < settings.ExtraDiceProb.Evaluate(t))
        {
            Debug.Log($"[{Name}] Wants to roll extra dice");
            diceCount++;
        }

        diceCount = Mathf.Min(diceHeld, Mathf.Clamp(diceCount, settings.MinDicePerTurn, settings.MaxDicePerTurn));
        diceHeld -= diceCount;
        diceValues = new int[diceCount];

        var text = $"Will roll **{diceValues.Length} dice**";
        Debug.Log($"[{Name}] {text}");
        StatusText.text = text;
        OnReport?.Invoke(this, text);
    }

    void RevealDiceValues()
    {
        for (int i = 0,l=actions.Count; i<l; i++)
        {
            actions[i].RevealValue();
        }
    }

    [SerializeField]
    Vector2 showActionsInitialOffset = Vector2.zero;
    [SerializeField]
    Vector2 showActionsDeltaOffset = new Vector2(0f, 1.1f);

    void ShowPossibleActions(bool updatePositions, MonsterAction activatedAction)
    {
        Debug.Log($"[{Name}] Showing / Reordering actions");

        var parentSize = (transform as RectTransform).rect.size;
        Vector2 offset = new Vector2(parentSize.x * showActionsInitialOffset.x, parentSize.y * showActionsInitialOffset.y); 

        for (int i = 0, l = actions.Count; i<l; i++)
        {
            var action = actions[i];
            var rt = action.transform as RectTransform;

            if (!action.IsOnCooldown && action.ActionPoints <= ActionPoints)
            {
                if (updatePositions)
                {
                    Debug.Log($"[{Name}] Could potentially use <{action.Name}> ({action.ValueRange} {action.ActionType})");
                    var childSize = rt.rect.size;
                    rt.SetParent(transform);
                    rt.offsetMax += offset;
                    rt.offsetMin += offset;


                    offset.x += childSize.x * showActionsDeltaOffset.x;
                    offset.y -= childSize.y * showActionsDeltaOffset.y;
                }

                action.gameObject.SetActive(true);

                rt.SetAsLastSibling();
            } else if (action != activatedAction)
            {
                HideAction(rt);
            }
        }
    }

    void HideAction(RectTransform rt)
    {
        rt.SetParent(actionsRoot);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.gameObject.SetActive(false);
    }

    void HideAllActions()
    {
        Debug.Log($"[{Name}] Hiding all actions");
        for (int i = 0, l = actions.Count; i<l; i++)
        {
            var rt = actions[i].transform as RectTransform;
            HideAction(rt);
        }
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
        ActionPoints = settings.ActionPointsPerTurn;

        DefenceText.text = BaseDefence.ToString();
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
        if (!Alive && KillGate.Open(out bool toggled))
        {
            if (toggled) gameObject.SetActive(false);
        }
    }
}
