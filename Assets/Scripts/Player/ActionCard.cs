using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public delegate void ActionCardEvent(ActionCard card, Monster reciever, int damage);

public enum UtilityType { Heal };

public class ActionCard : MonoBehaviour
{
    public static event ActionCardEvent OnAction;

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
    GameObject HoverEffect;

    [SerializeField, Range(0, 4)]
    int actionPointCost = 1;
    public int ActionPoints => actionPointCost;

    List<DieDropZone> dropZones = new List<DieDropZone>();

    [SerializeField]
    GameObject FaceUp;

    [SerializeField]
    GameObject FaceDown;

    public string ItemName => TitleUI.text;
    public bool Interactable { get; set; }
    public bool FacingUp {
        get => FaceUp.activeSelf;
        private set
        {
            FaceUp.SetActive(value);
            FaceDown.SetActive(!value);
            if (!value)
            {
                Interactable = false;
                HideHover();
            }
        }
    }

    [SerializeField]
    DieDropZone dropZonePrefab;

    [SerializeField]
    Transform dropZonesParent;

    [SerializeField]
    Image ActionTypeUI;

    [SerializeField]
    Sprite AttackSprite;

    [SerializeField]
    Sprite DefenceSprite;

    [SerializeField]
    Sprite HealSprite;

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

        switch (settings.ActionType)
        {
            case ActionType.Attack:
                ActionTypeUI.sprite = AttackSprite;
                break;
            case ActionType.Defence:
                ActionTypeUI.sprite = DefenceSprite;
                break;
            case ActionType.Healing:
                ActionTypeUI.sprite = HealSprite;
                break;
            default:
                ActionTypeUI.sprite = null;
                Debug.LogWarning($"[Action Card] {settings.Name} is of type {settings.ActionType}, don't know how to show that");
                break;
        }
    }

    private IEnumerable<DieDropZone> VisibleZones => dropZones.Where(dz => dz.gameObject.activeSelf);
    public int Value => VisibleZones.Sum(dz => dz.Value);

    public int OpenSlots => VisibleZones.Count(dz => dz.CanTakeDie);

    private void Awake()
    {
        dropZones.AddRange(GetComponentsInChildren<DieDropZone>(true));

        foreach(var dz in dropZones)
        {
            dz.Clear();
        }

        SyncSlottedDice();
    }

    private void OnEnable()
    {
        HideHover();
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
        } else if (phase == BattlePhase.PlayerAttack && actionType == ActionType.Attack && Value <= 0)
        {
            FacingUp = false;
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
        if (Interactable) HoverEffect.SetActive(true);
    }

    public void HideHover()
    {
        if (DraggedCard != this) HoverEffect.SetActive(false);
    }

    public void StartDrag()
    {
        if (Interactable && DraggedCard == null)
        {
            HoverEffect.SetActive(true);
            DraggedCard = this;
        }
    }

    public void EndDrag()
    {
        DraggedCard = null;
        HideHover();

        if (Interactable && actionType == ActionType.Attack && Monster.HoveredMonster != null)
        {
            FacingUp = false;
            var defence = Monster.HoveredMonster?.ConsumeDefenceForAttack(Value) ?? 0;
            var damage = Mathf.Max(0, Value - defence);
            if (Monster.HoveredMonster != null) {
                Monster.HoveredMonster.Health -= damage;
            }

            OnAction?.Invoke(this, Monster.HoveredMonster, damage);
        }
    }

    public void OnClick()
    {
        /*
        if (Interactable && actionType != ActionType.Attack)
        {
            FacingUp = false;
            OnAction?.Invoke(this, null);
        }
        */
    }
}
