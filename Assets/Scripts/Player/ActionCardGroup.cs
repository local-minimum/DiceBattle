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
    IEnumerable<ActionCard> ActiveCards => actionCards.Where((c, i) => i < GameProgress.CardHandSize && c.FacingUp);

    public int ActionPoints
    {
        get; private set;
    }

    public int SlottablePositions => ActiveCards.Sum(c => c.OpenSlots);
    public int Defence => ActiveCards.Sum(c => c.Action == ActionType.Defence ? c.Value : 0);

    private void OnEnable()
    {
        actionCards.Clear();
        actionCards.AddRange(GetComponentsInChildren<ActionCard>());
        SlotedDiceCache.Clear();

        Battle.OnChangePhase += Battle_OnChangePhase;
        ActionCard.OnAction += ActionCard_OnAction;
        Battle.OnBeginBattle += Battle_OnBeginBattle;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
        Battle.OnBeginBattle -= Battle_OnBeginBattle;
        ActionCard.OnAction -= ActionCard_OnAction;
    }

    private void Battle_OnBeginBattle()
    {
        SlotedDiceCache.Clear();
    }

    private void ActionCard_OnAction(ActionCard card, Monster reciever, int damage)
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

    Dictionary<int, List<int>> SlotedDiceCache = new Dictionary<int, List<int>>();

    void DrawHand()
    {
        var idx = 0;
        foreach (var (cardId, cardSettings) in ActionDeck.instance.Draw(GameProgress.CardHandSize))
        {
            Debug.Log($"{idx}: {cardSettings.Name}");
            var card = GetCard(idx);

            card.Store(ref SlotedDiceCache);
            card.Configure(cardId, cardSettings, SlotedDiceCache.GetValueOrDefault(cardId));

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
            if (card.FacingUp && card.ActionPoints <= ActionPoints && card.Action == ActionType.Attack && card.Value > 0)
            {
                anyInteractable = true;
                card.Interactable = true;
            } else
            {
                card.Interactable = false;
            }
        }

        if (!anyInteractable && Battle.Phase == BattlePhase.PlayerAttack)
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
            if (Battle.Phase == BattlePhase.PlayerAttack)
            {
                Battle.Phase = Battle.Phase.NextPhase();
            }
        }
    }
}
