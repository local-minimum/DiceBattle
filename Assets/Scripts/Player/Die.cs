using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void DropDieEvent(Die die);
public delegate void AutoslotDieEvent(Die die);
public delegate void TrashDieEvent(Die die);

public class Die : MonoBehaviour
{
    public static event DropDieEvent OnDropDie;
    public static event AutoslotDieEvent OnAutoslotDie;
    public static event TrashDieEvent OnTrashDie;

    public static Die DraggedDie { get; private set; }

    DiceManager diceManager;

    [SerializeField]
    TMPro.TextMeshProUGUI DieText;

    [SerializeField]
    Button button;

    [SerializeField]
    Image buttonImage;

    [SerializeField, Range(0, 1)]
    float doubleClickTime = 0.2f;

    [SerializeField, Range(0, 1)]
    float draggingSize = 0.5f;

    private bool interactable;
    public bool Interactable { 
        get => interactable; 
        set {
            interactable = value;
            button.interactable = value;
        }
    }

    public bool Rolled { get; set; }

    private void Start()
    {
        button = GetComponent<Button>();
        diceManager = GetComponentInParent<DiceManager>();
    }

    bool hovering = false;

    public bool RollCandidate
    {
        set
        {
            DieText.text = value ? "+" : "";
        }
    }

    public void OnHoverStart()
    {
        if (!button.interactable) return;

        hovering = this;

        if (Battle.Phase == BattlePhase.SelectNumberOfDice)
        {
            diceManager.SetRollCandidates(this);
        }
    }

    public void OnHoverEnd()
    {
        if (!Interactable) return;

        if (hovering && Battle.Phase == BattlePhase.SelectNumberOfDice)
        {
            diceManager.SetRollCandidates(null);
            hovering = false;
        }
    }


    #region Dragging
    [SerializeField, Range(0, 1), Tooltip("Time after release of drag die lingers in drag state")]
    float dragFuzzTime = 0.3f;

    RectTransform _canvasTransform;
    RectTransform CanvasTransform 
    {
        get
        {
            if (_canvasTransform == null)
            {
                _canvasTransform = GetComponentInParent<Canvas>().transform as RectTransform;
            }
            return _canvasTransform;
        }
    }

    int _postDragRestoreIndex = 0;
    RectTransform RestoreParentTransform { get; set; }

    float _stopDragTime = 0;
    bool Dragging
    {
        get => DraggedDie == this || Time.timeSinceLevelLoad < _stopDragTime;

        set
        {
            if (value)
            {
                if (DraggedDie != null && DraggedDie != this)
                {
                    DraggedDie.Dragging = false;
                }
                DraggedDie = this;
                buttonImage.raycastTarget = false;
            }
            else {
                if (DraggedDie == this)
                {
                    DraggedDie = null;
                }
                buttonImage.raycastTarget = true;
                _stopDragTime = Time.timeSinceLevelLoad + dragFuzzTime;
            }
        }
    }

    public void OnDragStart()
    {
        if (Battle.Phase != BattlePhase.UseDice || !Interactable) return;

        if (RestoreParentTransform == null)
        {
            _postDragRestoreIndex = transform.GetSiblingIndex();
            RestoreParentTransform = transform.parent as RectTransform;
        }
        transform.SetParent(CanvasTransform, true);
        var rt = transform as RectTransform;
        rt.sizeDelta = rt.sizeDelta * draggingSize;
        Dragging = true;
    }

    public void OnDragEnd()
    {
        if (!Dragging) return;

        transform.SetParent(RestoreParentTransform);
        transform.SetSiblingIndex(_postDragRestoreIndex);
        Dragging = false;
        OnDropDie?.Invoke(this);

        diceManager.CheckIfMoreDiceCanBeSlotted();
    }

    public void OnDrag()
    {
        if (!Dragging) return;

        transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
    #endregion

    float previousClick;
    public void OnClickDie()
    {
        if (Dragging || !Interactable) return;

        switch (Battle.Phase)
        {
            case BattlePhase.SelectNumberOfDice:
                diceManager.SelectDieToRoll(this);
                break;
            case BattlePhase.UseDice:
                var t = Time.timeSinceLevelLoad;
                if (t - previousClick < doubleClickTime)
                {
                    AutoslotDie();
                } else
                {
                    previousClick = t;
                }
                break;
        }
    }

    public void AutoslotDie()
    {
        if (!Interactable) return;

        OnAutoslotDie?.Invoke(this);

        diceManager.CheckIfMoreDiceCanBeSlotted();
    }

    public int Value { get; private set; }
    public void Roll()
    {
        Value = Random.Range(1, 7);
        buttonImage.sprite = Iconography.GetDie(Value);
        DieText.text = "";
        Rolled = true;
        transform.SetAsFirstSibling();
    }

    public void Clear()
    {
        Rolled = false;
        RollCandidate = false;
        transform.SetAsLastSibling();
    }

    public void NoDice()
    {
        Clear();
        Interactable = false;
        buttonImage.sprite = Iconography.GetDie(0);
    }

    public void HasDie()
    {
        transform.SetAsFirstSibling();
        Interactable = true;
    }


    public void TrashDie()
    {
        OnTrashDie?.Invoke(this);
        NoDice();
    }

    private void OnDestroy()
    {
        if (DraggedDie == this)
        {
            DraggedDie = null;
        }
    }
}
