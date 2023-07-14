using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void ActionCardEvent(ActionCard card);

public class ActionCard : MonoBehaviour
{
    public static event ActionCardEvent OnAction;

    [SerializeField]
    TMPro.TextMeshProUGUI SlottedDiceUI;

    [SerializeField]
    TMPro.TextMeshProUGUI ActionCostUI;

    [SerializeField]
    TMPro.TextMeshProUGUI TitleUI;

    [SerializeField]
    GameObject HoverEffect;

    [SerializeField, Range(0, 3)]
    int actionPointCost = 1;

    List<DieDropZone> dropZones = new List<DieDropZone>();

    [SerializeField]
    GameObject FaceUp;

    [SerializeField]
    GameObject FaceDown;

    public string ItemName => TitleUI.text;
    public bool Interactable { get; private set; }
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
        Interactable = actionPointCost > 0 && phase == BattlePhase.PlayerAttack;

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
        HoverEffect.SetActive(false);
    }

    public void OnClick()
    {
        if (Interactable)
        {
            FacingUp = false;

            Debug.Log($"Player performas a {Value} attack with {ItemName}");
            OnAction?.Invoke(this);
        }
    }
}
