using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeCrawl.Utils;
using UnityEngine;

public class ActionDeck : DeCrawl.Primitives.FindingSingleton<ActionDeck>
{
    [SerializeField]
    List<ActionCardSetting> Deck = new List<ActionCardSetting>();

    [SerializeField]
    bool ReshuffleEachSceneLoad = true;

    List<int> DrawPile = new List<int>();

    public IEnumerable<KeyValuePair<int, ActionCardSetting>> Draw(int count)
    {
        for (int i = 0; i<count; i++)
        {
            if (DrawPile.Count == 0)
            {
                ResetDeck();
            }

            var cardId = DrawPile[0];

            yield return new KeyValuePair<int, ActionCardSetting>(cardId, Deck[cardId]);

            DrawPile.Remove(cardId);
        }
    }

    void ResetDeck()
    {
        DrawPile = Enumerable.Range(0, Deck.Count).Shuffle().ToList();
    }

    private void Start()
    {
        if (ReshuffleEachSceneLoad)
        {
            ResetDeck();
        }
    }

    public IEnumerable<ActionCardSetting> Cards => Deck.OrderBy(c => c.ShopCost);

    public void RemoveOneInstance(ActionCardSetting setting)
    {
        for (int i = 0, l = Deck.Count; i<l; i++)
        {
            if (Deck[i] == setting)
            {
                Deck.RemoveAt(i);
                Debug.Log($"Removed card [{setting.name}] from player deck");
                ResetDeck();
                return;
            }
        }

        Debug.LogWarning($"Could not find any [{setting.Name}] in player deck to remove");
    }
}
