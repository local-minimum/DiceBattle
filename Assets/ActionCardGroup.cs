using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCardGroup : MonoBehaviour
{
    [SerializeField]
    int minActionsPerRound = 3;

    [SerializeField]
    int saveableActions = 0;

    ActionCard[] actionCards;

    public int ActionPoints
    {
        get; private set;
    }

    private void OnEnable()
    {
        actionCards = GetComponentsInChildren<ActionCard>();

        Battle.OnChangePhase += Battle_OnChangePhase;
        ActionCard.OnAction += ActionCard_OnAction;
    }


    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        ActionCard.OnAction -= ActionCard_OnAction;
    }

    private void ActionCard_OnAction(ActionCard card, Monster reciever)
    {
        ActionPoints -= card.ActionPoints;
        SyncCards();
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        triggerNextPhase = false;

        switch (phase)
        {
            case BattlePhase.PlayerAttack:
                SyncCards();
                break;
            case BattlePhase.Cleanup:
                RestoreActionPoints();
                break;
        }
    }

    void RestoreActionPoints()
    {
        ActionPoints = Mathf.Min(ActionPoints, saveableActions) + minActionsPerRound;
    }

    void SyncCards()
    {
        bool anyInteractable = false;
        for (int i = 0; i<actionCards.Length; i++)
        {
            var card = actionCards[i];
            if (card.ActionPoints <= ActionPoints && card.Action != ActionType.Passive)
            {
                anyInteractable = true;
                card.Interactable = true;
            } else
            {
                card.Interactable = false;
            }
        }

        if (!anyInteractable)
        {
            triggerTime = Time.timeSinceLevelLoad + triggerDelay;
            triggerNextPhase = true;
        }
    }

    [SerializeField]
    float triggerDelay = 0.5f;

    bool triggerNextPhase;
    float triggerTime;

    private void Update()
    {
        if (triggerNextPhase && triggerTime > Time.timeSinceLevelLoad)
        {
            triggerNextPhase = false;
            Battle.Phase = Battle.Phase.NextPhase();
        }
    }
}
