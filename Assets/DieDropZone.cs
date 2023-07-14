using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void RecycledDieEvent(int value);
public delegate void DieDropZoneChangeEvent(DieDropZone dropZone);

public class DieDropZone : MonoBehaviour
{
    public static event RecycledDieEvent OnRecycleDie;
    public static event DieDropZoneChangeEvent OnChange;

    [SerializeField]
    Color HoverColor;

    [SerializeField]
    Color DefaultColor;

    [SerializeField]
    Color ValueDefaultColor;

    [SerializeField]
    Color ValueDieColor;

    [SerializeField]
    float hoverScale = 1.1f;

    [SerializeField]
    Image backgroundImage;

    [SerializeField]
    int spawnValue = 0;

    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    bool holdsDie = false;
    public bool HoldsDie => holdsDie;

    bool hovered = false;
    public void OnHoverStart()
    {
        if (Die.DraggedDie != null)
        {
            ApplyEffect(HoverColor, hoverScale);
            hovered = true;
        }
    }

    public void OnHoverEnd()
    {
        if (hovered)
        {
            ApplyEffect(DefaultColor, 1f);
            hovered = false;
        }
    }

    void ApplyEffect(Color color, float padding)
    {
        backgroundImage.color = color;
        transform.localScale = Vector3.one * padding;
    }

    private void OnEnable()
    {
        holdsDie = false;
        Value = spawnValue;
        TextUI.color = ValueDefaultColor;

        Die.OnDropDie += Die_OnDropDie;
        Battle.OnChangePhase += Battle_OnChangePhase;

        OnChange?.Invoke(this);
    }

    private void OnDisable()
    {
        Die.OnDropDie -= Die_OnDropDie;
        Battle.OnChangePhase -= Battle_OnChangePhase;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        if (phase == BattlePhase.Cleanup) CleanUpPhase();
    }

    void CleanUpPhase()
    {
        if (holdsDie && Value > 1) { 
            Value--; 
            OnChange?.Invoke(this);
        }
    }

    private void Die_OnDropDie(Die die)
    {
        if (!hovered || !die.Interactable) return;

        if (holdsDie)
        {
            OnRecycleDie?.Invoke(Value);
        }

        Value = die.Value;
        TextUI.color = ValueDieColor;
        die.NoDice();
        holdsDie = true;

        OnChange?.Invoke(this);
    }

    int _value;
    public int Value { 
        get => _value; 
        private set
        {
            _value = value;
            TextUI.text = value.ToString();
        }
    }
}
