using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterActionsManager : MonoBehaviour
{
    [SerializeField] RectTransform MonsterActionsRoot;

    RectTransform RT;

    private void Start()
    {
        RT = transform as RectTransform;
    }

    [SerializeField]
    float showDuration = 1;

    Dictionary<float, MonsterAction> autohideCache = new Dictionary<float, MonsterAction>();

    void ShowAction(MonsterAction action)
    {
        action.RevealValue();

        action.transform.SetParent(RT);
        action.transform.SetAsLastSibling();
        action.gameObject.SetActive(true);
        autohideCache.Add(Time.timeSinceLevelLoad + showDuration, action);
    }

    void HideAction(MonsterAction action)
    {
        action.gameObject.SetActive(false);
        action.transform.SetParent(MonsterActionsRoot);
    }

    private void OnEnable()
    {
        MonsterAction.OnUse += MonsterAction_OnUse;
    }

    private void OnDisable()
    {
        MonsterAction.OnUse -= MonsterAction_OnUse;    
    }

    private void MonsterAction_OnUse(MonsterAction action)
    {
        ShowAction(action);
    }

    private void Update()
    {
        var times = autohideCache.Keys.ToArray();
        for(int i = 0; i < times.Length; i++)
        {
            var t = times[i];
            if (Time.timeSinceLevelLoad > t)
            {
                HideAction(autohideCache[t]);
                autohideCache.Remove(t);
            }
        }
    }
}
