using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void ActionCardEvent(ActionCard card, Monster reciever);

public enum ActionType { Attack, Defence, Passive }

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

    public int Value => dropZones.Sum(dz => dz.Value);

    public int OpenSlots => dropZones.Count(dz => dz.CanTakeDie);

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
            foreach (var dz in dropZones)
            {
                dz.CleanUp();
            }
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
            OnAction?.Invoke(this, Monster.HoveredMonster);
        }
    }

    public void OnClick()
    {
        if (Interactable && actionType != ActionType.Attack)
        {
            FacingUp = false;
            OnAction?.Invoke(this, null);
        }
    }
}
