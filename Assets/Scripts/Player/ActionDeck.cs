using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionDeck : MonoBehaviour
{
    [SerializeField]
    List<ActionCardSetting> Deck = new List<ActionCardSetting>();

    List<int> DrawPile = new List<int>();
    List<int> DiscardPile = new List<int>();

    public IEnumerable<KeyValuePair<int, ActionCardSetting>> Draw(int count)
    {
        for (int i = 0; i<count; i++)
        {
            if (DrawPile.Count == 0)
            {
                DrawPile.AddRange(DiscardPile);
            }

            var cardId = DrawPile[Random.Range(0, DrawPile.Count)];

            yield return new KeyValuePair<int, ActionCardSetting>(cardId, Deck[cardId]);

            DrawPile.Remove(cardId);
            DiscardPile.Add(cardId);
        }
    }

    private void OnEnable()
    {
        DiscardPile.Clear();
        DrawPile = Enumerable.Range(0, Deck.Count).ToList();
    }
}
