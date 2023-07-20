using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionCardGroup : MonoBehaviour
{
    [SerializeField]
    int minActionsPerRound = 3;

    [SerializeField]
    int saveableActions = 0;

    List<ActionCard> actionCards = new List<ActionCard>();

    [SerializeField]
    int handSize = 2;

    public int ActionPoints
    {
        get; private set;
    }

    public int SlottablePositions => actionCards.Sum(c => c.OpenSlots);

    private void OnEnable()
    {
        actionCards.AddRange(GetComponentsInChildren<ActionCard>());

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
                DrawHand();
                RestoreActionPoints();
                break;
        }
    }

    [SerializeField]
    ActionCard CardPrefab;

    [SerializeField]
    ActionDeck deck;

    ActionCard GetCard(int idx)
    {
        if (idx >= actionCards.Count)
        {
            var card = Instantiate(CardPrefab, transform);

            actionCards.Add(card);

            return card;
        }

        return actionCards[idx];
    }

    Dictionary<ActionCardSetting, List<int>> SlotedDiceCache = new Dictionary<ActionCardSetting, List<int>>();

    void DrawHand()
    {
        var idx = 0;
        foreach (var cardSettings in deck.Draw(handSize))
        {
            Debug.Log($"{idx}: {cardSettings.Name}");
            var card = GetCard(idx);
            card.Configure(cardSettings, SlotedDiceCache.GetValueOrDefault(cardSettings));
            card.gameObject.SetActive(true);
            idx++;
        }

        for (; idx < actionCards.Count; idx++)
        {
            actionCards[idx].gameObject.SetActive(false);
        }
    }

    void RestoreActionPoints()
    {
        ActionPoints = Mathf.Min(ActionPoints, saveableActions) + minActionsPerRound;
    }

    void SyncCards()
    {
        bool anyInteractable = false;
        for (int i = 0; i<actionCards.Count; i++)
        {
            var card = actionCards[i];
            if (card.FacingUp && card.ActionPoints <= ActionPoints && card.Action == ActionType.Attack)
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
