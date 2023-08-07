using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeCrawl.Utils;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [System.Serializable]
    struct IncreaseRollSize
    {
        public int[] CostByRollSize;
        public Sprite Sprite;

        public string Name;

        public bool CanIncrease => CostByRollSize.Length > GameProgress.RollSize;

        public int Cost => CostByRollSize[GameProgress.RollSize];
    }

    enum ShopCardType { Dice, RollSize, Potion, NewCard, DestroyCard, None }

    [SerializeField]
    ShopItemCard prefab;

    [SerializeField]
    Transform itemsRoot;

    [SerializeField]
    ShopPlayerStats playerStats;

    [SerializeField]
    int OtherGroupsPriorityBonus = 3;

    [SerializeField]
    HealthPotion[] Potions;

    List<int> AvailablePotions;
    int PotionsPriority = 0;

    [SerializeField]
    NewDiceCard[] Dice;

    [SerializeField]
    IncreaseRollSize increaseRollSize;
    bool UsedIncreaseRollSize;
    int IncreaseRollSizePriority = 0;

    List<int> AvailableDice;
    int DicePriority = 0;

    void PrepareShopSeeding()
    {
        AvailablePotions = Enumerable
            .Range(0, Potions.Length)
            .Shuffle()
            .ToList();
        PotionsPriority = 0;

        AvailableDice = Enumerable
            .Range(0, Dice.Length)
            .Shuffle()
            .ToList();
        DicePriority = 0;

        UsedIncreaseRollSize = false;
        IncreaseRollSizePriority = 0;
    }

    ShopCardType GetCardType()
    {
        int total = 0;

        int potions = AvailablePotions.Count;
        if (potions > 0)
        {
            total += potions + PotionsPriority;
        }

        int dice = AvailableDice.Count;
        if (dice > 0)
        {
            total += dice + DicePriority;
        }

        if (!UsedIncreaseRollSize && increaseRollSize.CanIncrease)
        {
            total += 1 + IncreaseRollSizePriority;
        }

        var value = Random.Range(0, total);

        if (potions > 0)
        {
            if (value < potions + PotionsPriority)
            {
                return ShopCardType.Potion;
            }
            value -= potions + PotionsPriority;
        }

        if (dice > 0)
        {
            if (value < dice + DicePriority)
            {
                return ShopCardType.Dice;
            }

            value -= dice + DicePriority;
        }

        if (!UsedIncreaseRollSize && increaseRollSize.CanIncrease)
        {
            if (value < 1 + IncreaseRollSizePriority)
            {
                return ShopCardType.RollSize;
            }

            value -= 1 + IncreaseRollSizePriority;
        }

        return ShopCardType.None;
    }

    void AllocatePriorities(ShopCardType cardType)
    {
        if (cardType == ShopCardType.None) return;

        if (cardType != ShopCardType.Potion) PotionsPriority += OtherGroupsPriorityBonus;
        if (cardType != ShopCardType.Dice) DicePriority += OtherGroupsPriorityBonus;
        if (cardType != ShopCardType.RollSize) IncreaseRollSizePriority += OtherGroupsPriorityBonus;
    }

    List<ShopItemCard> cards = new List<ShopItemCard>();

    ShopItemCard GetCard(int index)
    {
        if (index < cards.Count)
        {
            return cards[index];
        }

        var card = Instantiate(prefab, itemsRoot);
        cards.Add(card);

        return card;
    }

    void ValidateCardCosts()
    {

        for (int i = 0, l = cards.Count; i<l; i++)
        {
            cards[i].ValidateCost();
        }
    }

    void PickCardByType(ShopCardType cardType, int index)
    {
        var card = GetCard(index);

        switch (cardType)
        {
            case ShopCardType.Potion:
                var potion = Potions[AvailablePotions[0]];
                AvailablePotions.RemoveAt(0);
                card.Prepare(
                    potion.Name,
                    $"Heals: {potion.Amount}",
                    potion.Sprite,
                    potion.Cost,
                    delegate
                    {
                        GameProgress.Health += potion.Amount;
                        GameProgress.XP -= potion.Cost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            case ShopCardType.Dice:
                var dice = Dice[AvailableDice[0]];
                AvailableDice.RemoveAt(0);
                card.Prepare(
                    dice.Name,
                    "Adds new dice",
                    dice.Sprite,
                    dice.Cost,
                    delegate
                    {
                        GameProgress.Dice += dice.Dice;
                        GameProgress.XP -= dice.Cost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            case ShopCardType.RollSize:
                var cost = increaseRollSize.Cost;
                UsedIncreaseRollSize = true;
                card.Prepare(
                    increaseRollSize.Name,
                    "Increase number of dice that can be rolled",
                    increaseRollSize.Sprite,
                    cost,
                    delegate
                    {
                        GameProgress.IncreaseRollSize();
                        GameProgress.XP -= cost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            default:
                Debug.Log($"Don't know card type {cardType}");
                break;
        }
    }

    void SeedShop()
    {
        for (int i = 0; i < GameSettings.MaxShowWares; i++)
        {
            var cardType = GetCardType();
            AllocatePriorities(cardType);
            PickCardByType(cardType, i);
        }
    }

    private void Start()
    {
        cards.AddRange(itemsRoot.GetComponentsInChildren<ShopItemCard>(true));

        PrepareShopSeeding();
        SeedShop();

    }
}
