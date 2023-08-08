using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeCrawl.Utils;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [System.Serializable]
    struct FixedIncreaseStatCost
    {
        public int[] CostBySize;
        public Sprite Sprite;

        public string Name;

        public bool CanIncrease(int currentSize) => CostBySize.Length > currentSize + 1;

        public int Cost(int nextSize) => CostBySize[nextSize];
    }
    
    enum ShopCardType { Dice, RollSize, Potion, NewCard, DestroyCard, HandSize, None }

    [SerializeField, Range(0, 3)]
    float maxPrizeXPFactor = 1.25f;

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
    FixedIncreaseStatCost increaseRollSize;
    bool UsedIncreaseRollSize;
    int RollSizePriority = 0;

    [SerializeField]
    FixedIncreaseStatCost increaseHandSize;
    bool UsedIncreaseHandSize;
    int HandSizePriority = 0;

    [SerializeField]
    int MaxDestroyCards = 2;
    [SerializeField]
    int DestroySampleFromCheapest = 5;
    [SerializeField, Range(0, 10)]
    float DestroyCostMultiplier = 0.5f;
    List<ActionCardSetting> DestructableCards;
    int DestructablePriority = 0;

    List<int> AvailableDice;
    int DicePriority = 0;

    void PrepareShopSeeding()
    {
        int maxPrize = Mathf.RoundToInt(maxPrizeXPFactor * GameProgress.XP);

        AvailablePotions = Enumerable
            .Range(0, Potions.Length)
            .Where(idx => Potions[idx].Cost <= maxPrize)
            .Shuffle()
            .ToList();
        PotionsPriority = 0;

        AvailableDice = Enumerable
            .Range(0, Dice.Length)
            .Where(idx => Dice[idx].Cost <= maxPrize)
            .Shuffle()
            .ToList();
        DicePriority = 0;

        DestructableCards = ActionDeck.instance.Cards
            .Take(DestroySampleFromCheapest)
            .Where(setting => Mathf.RoundToInt(DestroyCostMultiplier * setting.ShopCost) <= maxPrize)
            .Shuffle()
            .Take(MaxDestroyCards)
            .ToList();
        DestructablePriority = 0;

        UsedIncreaseRollSize = increaseRollSize.Cost(GameProgress.RollSize + 1) > maxPrize;
        RollSizePriority = 0;

        UsedIncreaseHandSize = increaseHandSize.Cost(GameProgress.CardHandSize + 1) > maxPrize;
        HandSizePriority = 0;
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

        if (!UsedIncreaseRollSize && increaseRollSize.CanIncrease(GameProgress.RollSize))
        {
            total += 1 + RollSizePriority;
        }

        if (!UsedIncreaseHandSize && increaseHandSize.CanIncrease(GameProgress.CardHandSize))
        {
            total += 1 + RollSizePriority;
        }

        int destructables = DestructableCards.Count;
        if (destructables > 0)
        {
            total += destructables + DestructablePriority;
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

        if (!UsedIncreaseRollSize && increaseRollSize.CanIncrease(GameProgress.RollSize))
        {
            if (value < 1 + RollSizePriority)
            {
                return ShopCardType.RollSize;
            }

            value -= 1 + RollSizePriority;
        }

        if (!UsedIncreaseHandSize && increaseHandSize.CanIncrease(GameProgress.CardHandSize))
        {
            if (value < 1 + HandSizePriority)
            {
                return ShopCardType.HandSize;
            }

            value -= 1 + HandSizePriority;
        }

        if (destructables > 0)
        {
            if (value < destructables + DestructablePriority)
            {
                return ShopCardType.DestroyCard;
            }
            value -= destructables + DestructablePriority;
        }

        return ShopCardType.None;
    }

    void AllocatePriorities(ShopCardType cardType)
    {
        if (cardType == ShopCardType.None) return;

        if (cardType != ShopCardType.Potion) PotionsPriority += OtherGroupsPriorityBonus;
        if (cardType != ShopCardType.Dice) DicePriority += OtherGroupsPriorityBonus;
        if (cardType != ShopCardType.RollSize) RollSizePriority += OtherGroupsPriorityBonus;
        if (cardType != ShopCardType.HandSize) HandSizePriority += OtherGroupsPriorityBonus;
        if (cardType != ShopCardType.DestroyCard) DestructablePriority += OtherGroupsPriorityBonus;
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
                var rollCost = increaseRollSize.Cost(GameProgress.RollSize + 1);
                UsedIncreaseRollSize = true;
                card.Prepare(
                    increaseRollSize.Name,
                    "Increase number of dice that can be rolled",
                    increaseRollSize.Sprite,
                    rollCost,
                    delegate
                    {
                        GameProgress.IncreaseRollSize();
                        GameProgress.XP -= rollCost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            case ShopCardType.HandSize:
                var handCost = increaseHandSize.Cost(GameProgress.CardHandSize + 1);
                UsedIncreaseHandSize = true;
                card.Prepare(
                    increaseHandSize.Name,
                    "Increase number of cards in play per turn",
                    increaseHandSize.Sprite,
                    handCost,
                    delegate
                    {
                        GameProgress.IncreaseCardHandSize();
                        GameProgress.XP -= handCost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            case ShopCardType.DestroyCard:
                var setting = DestructableCards[0];
                DestructableCards.RemoveAt(0);
                int destroyCost = Mathf.RoundToInt(DestroyCostMultiplier * setting.ShopCost);
                card.Prepare(
                    $"DESTROY: {setting.Name}",
                    setting.Summary(),
                    setting.Sprite,
                    destroyCost,
                    delegate
                    {
                        ActionDeck.instance.RemoveOneInstance(setting);
                        GameProgress.XP -= destroyCost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            default:
                Debug.Log($"Don't know card type {cardType}");
                card.Flip();
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
