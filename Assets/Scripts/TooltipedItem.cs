using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine;

public delegate void TooltipEvent(TooltipedItem tooltip, bool show);

public class TooltipedItem : MonoBehaviour
{
    [SerializeField]
    string tooltip;
    public string Tooltip => tooltip;

    public void SetTooltip(string value)
    {
        tooltip = value;

        if (hovered) OnTooltip?.Invoke(this, true);
    }

    public static event TooltipEvent OnTooltip;

    EventTrigger _eventTrigger;
    EventTrigger EventTrigger
    {
        get
        {
            if (_eventTrigger == null)
            {
                _eventTrigger = GetComponentInChildren<EventTrigger>(true);

                if (_eventTrigger == null)
                {
                    _eventTrigger = gameObject.AddComponent<EventTrigger>();
                }
            }
            return _eventTrigger;
        }
    }

    EventTrigger.Entry pointerEnterEntry;
    EventTrigger.Entry pointerExitEntry;

    private void OnEnable()
    {
        var trigger = EventTrigger;

        if (pointerEnterEntry == null)
        {
            pointerEnterEntry = new EventTrigger.Entry();
            pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
            pointerEnterEntry.callback.AddListener(delegate { ShowTooltip(); });
        }

        if (pointerExitEntry == null)
        {
            pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener(delegate { HideTooltip(); });
        }

        trigger.triggers.Add(pointerEnterEntry);
        trigger.triggers.Add(pointerExitEntry);
    }

    private void OnDisable()
    {
        var trigger = EventTrigger;
        trigger.triggers.Remove(pointerEnterEntry);
        trigger.triggers.Remove(pointerExitEntry);
    }

    bool hovered = false;

    void ShowTooltip()
    {
        hovered = true;
        OnTooltip?.Invoke(this, true);
    }

    void HideTooltip()
    {
        hovered = false;
        OnTooltip?.Invoke(this, false);
    }
}
