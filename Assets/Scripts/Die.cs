using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void DropDieEvent(Die die);

public class Die : MonoBehaviour
{
    public static event DropDieEvent OnDropDie;

    public static Die DraggedDie { get; private set; }

    DiceManager diceManager;

    [SerializeField]
    TMPro.TextMeshProUGUI DieText;

    [SerializeField]
    Button button;

    [SerializeField]
    Image buttonImage;

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
        if (Battle.Phase != BattlePhase.UseDice) return;

        if (RestoreParentTransform == null)
        {
            _postDragRestoreIndex = transform.GetSiblingIndex();
            RestoreParentTransform = transform.parent as RectTransform;
        }
        transform.SetParent(CanvasTransform, true);
        Dragging = true;
    }

    public void OnDragEnd()
    {
        if (!Dragging) return;

        transform.SetParent(RestoreParentTransform);
        transform.SetSiblingIndex(_postDragRestoreIndex);
        Dragging = false;
        OnDropDie?.Invoke(this);

        if (!diceManager.HasDiceRemaining())
        {
            Battle.Phase = Battle.Phase.NextPhase();
        }
    }

    public void OnDrag()
    {
        if (!Dragging) return;

        transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
    #endregion

    public void OnClickDie()
    {
        if (Dragging || !Interactable) return;

        switch (Battle.Phase)
        {
            case BattlePhase.SelectNumberOfDice:
                diceManager.SelectDieToRoll(this);
                break;
        }
    }

    public int Value { get; private set; }
    public void Roll()
    {
        Value = Random.Range(1, 7);
        DieText.text = Value.ToString();
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
    }

    public void HasDie()
    {
        transform.SetAsFirstSibling();
        Interactable = true;
    }

    private void OnDestroy()
    {
        if (DraggedDie == this)
        {
            DraggedDie = null;
        }
    }
}
