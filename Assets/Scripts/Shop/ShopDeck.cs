using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopDeck : DeCrawl.Primitives.FindingSingleton<ShopDeck> 
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField]
    List<ActionCardSetting> Deck = new List<ActionCardSetting>();

    public IEnumerable<ActionCardSetting> Cards(int minCost, int maxCost) => Deck
        .Where(c => c.ShopCost >= minCost && c.ShopCost < maxCost)
        .OrderBy(c => c.ShopCost);

    public void RemoveOneInstance(ActionCardSetting setting)
    {
        for (int i = 0, l = Deck.Count; i<l; i++)
        {
            if (Deck[i] == setting)
            {
                Deck.RemoveAt(i);
                Debug.Log($"Removed card [{setting.name}] from shop deck");
                return;
            }
        }

        Debug.LogWarning($"Could not find any [{setting.Name}] in shop deck to remove");
    }
}
