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

    Dictionary<float, MonsterAction> hideCache = new Dictionary<float, MonsterAction>();

    public void ShowAction(MonsterAction action)
    {
        action.transform.SetParent(RT);
        action.transform.SetAsLastSibling();
        action.gameObject.SetActive(true);
        hideCache.Add(Time.timeSinceLevelLoad + showDuration, action);
    }

    public void HideAction(MonsterAction action)
    {
        action.gameObject.SetActive(false);
        action.transform.SetParent(MonsterActionsRoot);
    }

    private void Update()
    {
        var times = hideCache.Keys.ToArray();
        for(int i = 0; i < times.Length; i++)
        {
            var t = times[i];
            if (Time.timeSinceLevelLoad > t)
            {
                HideAction(hideCache[t]);
                hideCache.Remove(t);
            }
        }
    }
}
