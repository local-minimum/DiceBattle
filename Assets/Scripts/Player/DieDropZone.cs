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
    Image dieImage;

    [SerializeField]
    int spawnValue = 0;

    [SerializeField]
    DieEffect dieEffect = DieEffect.Add;
    public DieEffect DieEffect => dieEffect;

    [SerializeField]
    TMPro.TextMeshProUGUI TextUI;

    bool holdsDie = false;
    public bool HoldsDie => holdsDie;

    Color CurrentColor
    {
        get
        {
            if (!CanTakeDie)
            {
                return ValueLockedDieColor;
            } else if (HoldsDie)
            {
                return ValueDieColor;
            } else
            {
                return ValueDefaultColor;
            }
        }
    }

    private bool _canTakeDie;
    public bool CanTakeDie
    {
        get => _canTakeDie;
        private set
        {
            _canTakeDie = value;
            TextUI.color = CurrentColor;
            backgroundImage.color = TextUI.color;

            if (!HoldsDie)
            {
                dieImage.sprite = null;
            }
        }
    }

    public void Configure(DieEffect effect, int value, bool holdsDie)
    {
        dieEffect = effect;
        this.holdsDie = holdsDie;
        DiceValue = value;

        CanTakeDie = true;
    }


    static DieDropZone HoveredZone;

    private bool Hovered
    {
        get => HoveredZone == this;
        set
        {
            if (value)
            {
                if (HoveredZone != null)
                {
                    HoveredZone.OnHoverEnd();
                }
                HoveredZone = this;
            } else
            {
                if (HoveredZone == this)
                {
                    HoveredZone = null;
                }
            }
        }
    }

    public void OnHoverStart()
    {
        if (Die.DraggedDie != null && CanTakeDie)
        {
            ApplyEffect(HoverColor, hoverScale);
            Hovered = this;
        }
    }

    public void OnHoverEnd()
    {
        if (Hovered && CanTakeDie)
        {
            Hovered = false;
            ApplyEffect(CurrentColor, 1f);
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

        OnChange?.Invoke(this);
    }

    private void OnEnable()
    {
        Die.OnDropDie += Die_OnDropDie;
        Battle.OnBeginBattle += Battle_OnBeginBattle;

        OnChange?.Invoke(this);
    }

    private void OnDisable()
    {
        Die.OnDropDie -= Die_OnDropDie;
        Battle.OnBeginBattle -= Battle_OnBeginBattle;
        Hovered = false;
    }


    private void Battle_OnBeginBattle()
    {
        Clear();
    }

    public void CleanUp()
    {
        CanTakeDie = true;
        Hovered = false;

        if (holdsDie && DiceValue > 1) { 
            DiceValue--; 
            OnChange?.Invoke(this);
        }
    }

    private void Die_OnDropDie(Die die)
    {
        if (!Hovered) return;
        TakeDie(die);
    }

    public bool TakeDie(Die die)
    {
        if (!CanTakeDie || !die.Interactable) return false;

        if (holdsDie)
        {
            OnRecycleDie?.Invoke(Value);
        }

        holdsDie = true;
        DiceValue = die.Value;
        die.NoDice();
        CanTakeDie = false;

        OnChange?.Invoke(this);

        return true;
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
            TextUI.text = "";
            if (HoldsDie)
            {
                dieImage.sprite = Iconography.GetDie(Value);
                TextUI.text = "";
            } else
            {
                dieImage.sprite = null;
                TextUI.text = Value.ToString();
            }
        }
    }
       
}
