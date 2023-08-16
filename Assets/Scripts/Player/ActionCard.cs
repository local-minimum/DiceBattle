using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public enum ActionCardStatus { DragStart, DragEnd, Click, Flip, Reveal };

public delegate void ActionCardEvent(ActionCard card, Monster reciever, int damage);
public delegate void ActionCardStatusEvent(ActionCard card, ActionCardStatus status);

public enum UtilityType { Heal };

public class ActionCard : MonoBehaviour
{
    static readonly string UsedReason = "Used";
    static readonly string TooWeakReason = "Too weak to cause damage";

    public static event ActionCardEvent OnAction;
    public static event ActionCardStatusEvent OnStatus;

    public static ActionCard DraggedCard { get; private set; }

    [SerializeField]
    ActionType actionType = ActionType.Attack;
    public ActionType Action => actionType;

    [SerializeField]
    TMPro.TextMeshProUGUI SlottedDiceUI;

    [SerializeField]
    TMPro.TextMeshProUGUI ActionCostUI;

    [SerializeField]
    TMPro.TextMeshProUGUI TitleUI;

    [SerializeField]
    Image ImageUI;

    [SerializeField]
    Color InteractableColor;

    [SerializeField]
    Color AutomaticColor;

    [SerializeField]
    Image OutlineEffect;

    [SerializeField, Range(0, 4)]
    int actionPointCost = 1;
    public int ActionPoints => actionPointCost;

    List<DieDropZone> dropZones = new List<DieDropZone>();

    [SerializeField]
    GameObject FaceUp;

    [SerializeField]
    GameObject FaceDown;

    [SerializeField]
    TMPro.TextMeshProUGUI FaceDownTitle;

    [SerializeField]
    TMPro.TextMeshProUGUI FaceDownReason;

    public string ItemName => TitleUI.text;
    public bool Interactable
    {
        get
        {
            switch (Battle.Phase)
            {
                case BattlePhase.PlayerAttack:
                    return Action == ActionType.Attack && FacingUp;
                default:
                    return false;
            }
        }
    } 

    public bool FacingUp {
        get => FaceUp.activeSelf;
        private set
        {
            Debug.Log($"[{ItemName}] Facing Up set to {value}");
            FaceUp.SetActive(value);
            FaceDown.SetActive(!value);
            if (!value && DraggedCard != this)
            {
                HideOutline();
            }
            OnStatus?.Invoke(this, value ? ActionCardStatus.Reveal : ActionCardStatus.Flip);
        }
    }

    [SerializeField]
    DieDropZone dropZonePrefab;

    [SerializeField]
    Transform dropZonesParent;

    [SerializeField]
    Image ActionTypeUI;

    DieDropZone GetSlot(int idx)
    {
        if (idx < dropZones.Count) return dropZones[idx];

        var slot = Instantiate(dropZonePrefab, dropZonesParent);
        dropZones.Add(slot);

        return slot;
    }

    int cardId;

    public void Store(ref Dictionary<int, List<int>> cache)
    {
        if (cardId < 0) return;

        List<int> dice = new List<int>();

        foreach (var dz in VisibleZones)
        {
            dz.CleanUp();
            dice.Add(dz.HoldsDie ? dz.Value : 0);
        }

        cache[cardId] = dice;

        cardId = -1;
    }

    public void Configure(int cardId, ActionCardSetting settings, List<int> dice)
    {
        this.cardId = cardId;

        TitleUI.text = settings.Name;

        ImageUI.sprite = settings.Sprite;
        ImageUI.color = settings.Sprite == null ? Color.black : Color.white;

        actionType = settings.ActionType;
        if (actionType != ActionType.Attack)
        {
            ShowOutline(AutomaticColor);
        } else
        {
            HideOutline(true);
        }

        int idx = 0;
        for (;idx<settings.Slots.Length; idx++)
        {
            var slotConfig = settings.Slots[idx];
            bool hasDie = dice != null && idx < dice.Count && dice[idx] != 0;
            int slotValue = hasDie ? dice[idx] : slotConfig.DefaultValue;
            var slot = GetSlot(idx);

            slot.Configure(slotConfig.Effect, slotValue, hasDie);

            dropZones[idx].gameObject.SetActive(true);
        }

        for (var l = dropZones.Count; idx<l; idx++)
        {
            dropZones[idx].gameObject.SetActive(false);
        }

        ActionTypeUI.sprite = Iconography.GetAction(settings.ActionType);
        switch (settings.ActionType)
        {
            case ActionType.Attack:
                FaceDownTitle.text = "Attack";
                break;
            case ActionType.Defence:
                FaceDownTitle.text = "Defence";
                break;
            case ActionType.Healing:
                FaceDownTitle.text = "Healing";
                break;
            default:
                ActionTypeUI.sprite = null;
                FaceDownTitle.text = "Unknown";
                Debug.LogWarning($"[Action Card] {settings.Name} is of type {settings.ActionType}, don't know how to show that");
                break;
        }
    }

    private IEnumerable<DieDropZone> VisibleZones => dropZones.Where(dz => dz.gameObject.activeSelf);
    public int Value => VisibleZones.Sum(dz => dz.Value);

    public int OpenSlots => VisibleZones.Count(dz => dz.CanTakeDie);

    public bool Autoslot(Die die)
    {
        foreach (var dz in VisibleZones.Where(dz => dz.CanTakeDie))
        {
            if (dz.TakeDie(die))
            {
                return true;
            }
        }
        return false;
    }

    private void Awake()
    {
        dropZones.AddRange(GetComponentsInChildren<DieDropZone>(true));

        foreach(var dz in dropZones)
        {
            dz.Clear();
        }

        FacingUp = true;

        SyncSlottedDice();
    }

    private void OnEnable()
    {
        SyncActionPointCost();
        DieDropZone.OnChange += DieDropZone_OnChange;
        Battle.OnChangePhase += Battle_OnChangePhase;
    }

    private void OnDisable()
    {
        DieDropZone.OnChange -= DieDropZone_OnChange;
        Battle.OnChangePhase -= Battle_OnChangePhase;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        if (phase == BattlePhase.Cleanup)
        {
            FacingUp = true;
        } else if (phase == BattlePhase.PlayerAttack && actionType == ActionType.Attack && (Value <= 0 || !MonsterManager.instance.CanAffectAnyMonster(this)))
        {
            FacingUp = false;
            FaceDownReason.text = TooWeakReason;
        }
    }

    private void DieDropZone_OnChange(DieDropZone dropZone)
    {
        if (!dropZones.Contains(dropZone)) return;

        SyncSlottedDice();
    }

    void SyncSlottedDice()
    {
        var slotted = dropZones.Where(dz => dz.HoldsDie).Count();
        SlottedDiceUI.text = $"{slotted} / {dropZones.Count}";
    }

    void SyncActionPointCost()
    {
        ActionCostUI.text = new string('X', actionPointCost);
    }

    public void ShowHover()
    {
        if (Interactable)
        {
            ShowOutline(InteractableColor);
        }
    }


    void ShowOutline(Color color)
    {
        OutlineEffect.color = color;
        OutlineEffect.gameObject.SetActive(true);
    }

    public void HideHover()
    {
        if (Interactable) HideOutline();
    }

    void HideOutline(bool force = false)
    {
        if (force || DraggedCard != this)
        {
            OutlineEffect.gameObject.SetActive(false);
        }
    }

    public void StartDrag()
    {
        if (Interactable && DraggedCard == null)
        {
            OutlineEffect.gameObject.SetActive(true);
            DraggedCard = this;
            OnStatus?.Invoke(this, ActionCardStatus.DragStart);
        }
    }

    public void EndDrag()
    {
        DraggedCard?.HideOutline(true);
        DraggedCard = null;
        HideOutline();

        Debug.Log($"[{TitleUI.text}] Hover end");
        OnStatus?.Invoke(this, ActionCardStatus.DragEnd);

        if (Interactable && actionType == ActionType.Attack && Monster.HoveredMonster != null)
        {
            InvokeAttack(Monster.HoveredMonster);
        }
    }
    
    void InvokeAttack(Monster target)
    {
        var defence = target?.ConsumeDefenceForAttack(Value) ?? 0;
        var damage = Mathf.Max(0, Value - defence);
        if (target != null) {
            target.Health -= damage;
        }

        Debug.Log($"[{TitleUI.text}] Invoking attack on <{target.Name}>");
        OnAction?.Invoke(this, target, damage);

        FaceDownReason.text = UsedReason;
        FacingUp = false;
    }

    public void OnClick()
    {
        if (!Interactable) return;

        var monster = MonsterManager.instance.FirstAffectableMonster(this);
        InvokeAttack(monster);

        OnStatus?.Invoke(this, ActionCardStatus.Click);
    }
}
