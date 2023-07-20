using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void RecycledDieEvent(int value);
public delegate void DieDropZoneChangeEvent(DieDropZone dropZone);

public enum DieEffect { Add, Subtract };

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
    Color ValueLockedDieColor;

    [SerializeField]
    float hoverScale = 1.1f;

    [SerializeField]
    Image backgroundImage;

    [SerializeField]
    int spawnValue = 0;

    [SerializeField]
    DieEffect dieEffect = DieEffect.Add;
    public DieEffect DieEffect => dieEffect;

    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    bool holdsDie = false;
    public bool HoldsDie => holdsDie;

    public bool CanTakeDie
    {
        get; private set;
    }


    bool hovered = false;
    public void OnHoverStart()
    {
        if (Die.DraggedDie != null && CanTakeDie)
        {
            ApplyEffect(HoverColor, hoverScale);
            hovered = true;
        }
    }

    public void OnHoverEnd()
    {
        if (hovered && CanTakeDie)
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

    public void Clear()
    {
        CanTakeDie = true;
        holdsDie = false;
        DiceValue = spawnValue;
        TextUI.color = ValueDefaultColor;

        OnChange?.Invoke(this);
    }

    private void OnEnable()
    {
        Die.OnDropDie += Die_OnDropDie;

        OnChange?.Invoke(this);
    }

    private void OnDisable()
    {
        Die.OnDropDie -= Die_OnDropDie;
    }


    public void CleanUp()
    {
        CanTakeDie = true;
        TextUI.color = holdsDie ? ValueDieColor : DefaultColor;

        if (holdsDie && DiceValue > 1) { 
            DiceValue--; 
            OnChange?.Invoke(this);
        }
    }

    private void Die_OnDropDie(Die die)
    {
        if (!CanTakeDie || !hovered || !die.Interactable) return;

        if (holdsDie)
        {
            OnRecycleDie?.Invoke(Value);
        }

        DiceValue = die.Value;
        TextUI.color = ValueLockedDieColor;
        die.NoDice();
        holdsDie = true;
        CanTakeDie = false;
        OnChange?.Invoke(this);
    }

    int _value;
    public int Value {
        get {
            switch (DieEffect) {
                case DieEffect.Add:
                    return _value;
                case DieEffect.Subtract:
                    return -_value;
                default:
                    return 0;
            }
        }
    }

    public int DiceValue
    {
        get => _value;
        set
        {
            _value = Mathf.Abs(value);
            TextUI.text = _value.ToString();
        }
    }
       
}
