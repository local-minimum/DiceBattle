using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Die : MonoBehaviour
{
    DiceManager diceManager;

    [SerializeField]
    TMPro.TextMeshProUGUI DieText;

    [SerializeField]
    Button button;

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

    public bool BeforeRollHovered
    {
        set
        {
            DieText.text = value ? "+" : "";
        }
    }

    public void OnHoverStart()
    {
        if (!button.interactable) return;

        if (diceManager.phase == DiceManagerPhases.PreRoll)
        {
            BeforeRollHovered = true;
            diceManager.StartHoverDie(this);
        }
    }

    public void OnHoverEnd()
    {
        if (!Interactable) return;

        /*
        BeforeRollHovered = false;
        diceManager.EndHoverDie(this);
        */
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

    bool _dragging = false;
    float _stopDragTime = 0;
    bool Dragging
    {
        get => _dragging || Time.timeSinceLevelLoad < _stopDragTime;

        set
        {
            _dragging = value;
            if (!value)
            {
                _stopDragTime = Time.timeSinceLevelLoad + dragFuzzTime;
            }
        }
    }

    public void OnDragStart()
    {
        if (diceManager.phase != DiceManagerPhases.Rolled) return;

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

        switch (diceManager.phase)
        {
            case DiceManagerPhases.PreRoll:
                Roll();
                diceManager.RollDie(this);
                break;
            case DiceManagerPhases.Rolled:
                diceManager.ResetDice();
                break;
        }
    }

    public void Roll()
    {
        DieText.text = Random.Range(1, 7).ToString();
        Rolled = true;
    }

    public void Clear()
    {
        Rolled = false;
        BeforeRollHovered = false;
    }

    public void NoDice()
    {
        Interactable = false;
    }

    public void HasDie()
    {
        Interactable = true;
    }
}
