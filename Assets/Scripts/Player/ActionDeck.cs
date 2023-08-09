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
        Debug.Log($"[Player Deck] Draw {count} ({DrawPile.Count} / {Deck.Count} remaining)");
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
        Debug.Log($"[Player Deck] Shuffle ({DrawPile.Count} / {Deck.Count} used)");
        DrawPile = Enumerable.Range(0, Deck.Count).Shuffle().ToList();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
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
                Debug.Log($"[Player Deck] Removing card [{setting.name}]");
                ResetDeck();
                return;
            }
        }

        Debug.LogWarning($"[Player Deck] Could not find any [{setting.Name}] to remove");
    }

    public void AddCard(ActionCardSetting setting)
    {
        Debug.Log($"[Player Deck] Adding [{setting.Name}]");
        Deck.Add(setting);
        ResetDeck();
    }
}
