using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDeck : MonoBehaviour
{
    [SerializeField]
    List<ActionCardSetting> Deck = new List<ActionCardSetting>();

    List<ActionCardSetting> UsedCards = new List<ActionCardSetting>();

    public IEnumerable<ActionCardSetting> Draw(int count)
    {
        for (int i = 0; i<count; i++)
        {
            if (Deck.Count == 0)
            {
                Deck.AddRange(UsedCards);
            }

            var idx = Random.Range(0, Deck.Count);
            var card = Deck[idx];
            yield return Deck[idx];

            Deck.Remove(card);
            UsedCards.Add(card);
        }
    }
}
