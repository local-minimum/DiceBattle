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

        public int Cost(int nextSize) => nextSize > CostBySize.Length ? int.MaxValue : CostBySize[nextSize];
    }
    
    enum ShopCardType { Dice, RollSize, Potion, NewCard, DestroyCard, HandSize, BaseDefence, MaxHealth, None }

    [Header("- General Settings -")]
    [SerializeField, Range(0, 3)]
    float maxPrizeXPFactor = 1.25f;

    [SerializeField]
    ShopItemCard prefab;

    [SerializeField]
    Transform itemsRoot;

    [SerializeField]
    ShopPlayerStats playerStats;

    [SerializeField, Range(1, 10)]
    int OtherGroupsPriorityBonus = 3;

    [Header("- Health Settings -")]
    [SerializeField]
    HealthPotion[] Potions;
    [SerializeField, Range(1, 5)]
    float NoHealthPotionBonus = 3;

    List<int> AvailablePotions;
    int PotionsPriority = 0;

    [SerializeField]
    FixedIncreaseStatCost increaseMaxHealth;
    bool UsedIncreaseMaxHealth;
    int MaxHealthPriority;

    [Header("- Defence Settings -")]
    [SerializeField]
    FixedIncreaseStatCost increaseBaseDefence;
    bool UsedIncreaseBaseDefence;
    int BaseDefencePriority;

    [Header("- Dice Settings -")]
    [SerializeField]
    NewDiceCard[] Dice;

    [SerializeField]
    FixedIncreaseStatCost increaseRollSize;
    bool UsedIncreaseRollSize;
    int RollSizePriority = 0;

    [Header("- Cards Settings -")]
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

    [SerializeField]
    float NewCardMinCostFactor = 0.9f;
    [SerializeField]
    int NewCardPlayerCardsRefSample = 3;
    [SerializeField, Range(0, 20)]
    int NewCardsSampleCount = 10;
    [SerializeField, Range(0, 10)]
    int NewCardsMax = 6;
    List<ActionCardSetting> NewCards;
    int NewCardsPriority = 0;

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
        Debug.Log($"[Shop] {AvailablePotions.Count} Potions available");
        PotionsPriority = 0;

        AvailableDice = Enumerable
            .Range(0, Dice.Length)
            .Where(idx => Dice[idx].Cost <= maxPrize)
            .Shuffle()
            .ToList();
        Debug.Log($"[Shop] {AvailableDice.Count} Dice available");
        DicePriority = 0;

        DestructableCards = ActionDeck.instance.Cards
            .Take(DestroySampleFromCheapest)
            .Where(setting => Mathf.RoundToInt(DestroyCostMultiplier * setting.ShopCost) <= maxPrize)
            .Shuffle()
            .Take(MaxDestroyCards)
            .ToList();
        Debug.Log($"[Shop] {DestructableCards.Count} Destructable cards available");
        DestructablePriority = 0;

        UsedIncreaseRollSize = increaseRollSize.Cost(GameProgress.RollSize + 1) > maxPrize;
        Debug.Log($"[Shop] Increase roll size {(UsedIncreaseHandSize ? "unavailable" : "available")}");
        RollSizePriority = 0;

        UsedIncreaseHandSize = increaseHandSize.Cost(GameProgress.CardHandSize + 1) > maxPrize;
        Debug.Log($"[Shop] Increase hand size {(UsedIncreaseHandSize ? "unavailable" : "available")}");
        HandSizePriority = 0;

        UsedIncreaseMaxHealth = increaseMaxHealth.Cost(GameProgress.IncreasedMaxHealth + 1) > maxPrize;
        Debug.Log($"[Shop] Increase max health {(UsedIncreaseMaxHealth ? "unavailable" : "available")}");
        MaxHealthPriority = 0;

        UsedIncreaseBaseDefence = increaseBaseDefence.Cost(GameProgress.BaseDefence + 1) > maxPrize;
        Debug.Log($"[Shop] Increase max health {(UsedIncreaseMaxHealth ? "unavailable" : "available")}");
        BaseDefencePriority = 0;


        var playerCardsRefCards = ActionDeck.instance.Cards
            .TakeLast(NewCardPlayerCardsRefSample)
            .ToList();

        var playerCardsRefCost = playerCardsRefCards.Sum(c => c.ShopCost) / playerCardsRefCards.Count;

        NewCards = ShopDeck.instance
            .Cards(Mathf.RoundToInt(NewCardMinCostFactor * playerCardsRefCost), GameProgress.XP)
            .Take(NewCardsSampleCount)
            .Shuffle()
            .Take(NewCardsMax)
            .ToList();
        Debug.Log($"[Shop] {NewCards.Count} New cards available");
        NewCardsPriority = 0;
    }

    ShopCardType GetCardType(bool noHealthPotion)
    {
        int total = 0;

        int potions = AvailablePotions.Count;
        int CalibratedPotionsPriority = noHealthPotion ? Mathf.RoundToInt(NoHealthPotionBonus * PotionsPriority) : PotionsPriority;
        if (potions > 0)
        {
            total += potions + CalibratedPotionsPriority;
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
            total += 1 + HandSizePriority;
        }

        if (!UsedIncreaseMaxHealth && increaseMaxHealth.CanIncrease(GameProgress.IncreasedMaxHealth))
        {
            total += 1 + MaxHealthPriority;
        }

        if (!UsedIncreaseBaseDefence && increaseBaseDefence.CanIncrease(GameProgress.BaseDefence))
        {
            total += 1 + BaseDefencePriority;
        }

        int destructables = DestructableCards.Count;
        if (destructables > 0)
        {
            total += destructables + DestructablePriority;
        }

        int newcards = NewCards.Count;
        if (newcards > 0)
        {
            total += newcards + NewCardsPriority;
        }

        var value = Random.Range(0, total);

        if (potions > 0)
        {
            if (value < potions + CalibratedPotionsPriority)
            {
                return ShopCardType.Potion;
            }
            value -= potions + CalibratedPotionsPriority;
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

        if (!UsedIncreaseMaxHealth && increaseMaxHealth.CanIncrease(GameProgress.IncreasedMaxHealth))
        {
            if (value < 1 + MaxHealthPriority)
            {
                return ShopCardType.MaxHealth;
            }

            value -= 1 + MaxHealthPriority;
        }

        if (!UsedIncreaseBaseDefence && increaseBaseDefence.CanIncrease(GameProgress.BaseDefence))
        {
            if (value < 1 + BaseDefencePriority)
            {
                return ShopCardType.BaseDefence;
            }

            value -= 1 + BaseDefencePriority;
        }

        if (destructables > 0)
        {
            if (value < destructables + DestructablePriority)
            {
                return ShopCardType.DestroyCard;
            }
            value -= destructables + DestructablePriority;
        }

        if (newcards > 0)
        {
            if (value < newcards + NewCardsPriority)
            {
                return ShopCardType.NewCard;
            }
            value -= newcards + NewCardsPriority;
        }

        Debug.LogWarning($"Failed to select a new card type (Remaining random value is {value})");
        return ShopCardType.None;
    }

    void AllocatePriorities(ShopCardType cardType)
    {
        if (cardType == ShopCardType.None) return;

        if (cardType != ShopCardType.Potion)
        {
            PotionsPriority += OtherGroupsPriorityBonus;
        } else
        {
            PotionsPriority = 0;
        }
        if (cardType != ShopCardType.Dice)
        {
            DicePriority += OtherGroupsPriorityBonus;
        } else
        {
            DicePriority = 0;
        }
        if (cardType != ShopCardType.RollSize)
        {
            RollSizePriority += OtherGroupsPriorityBonus;
        } else
        {
            RollSizePriority = 0;
        }
        if (cardType != ShopCardType.HandSize)
        {
            HandSizePriority += OtherGroupsPriorityBonus;
        } else
        {
            HandSizePriority = 0;
        }
        if (cardType != ShopCardType.DestroyCard)
        {
            DestructablePriority += OtherGroupsPriorityBonus;
        } else
        {
            DestructablePriority = 0;
        }
        if (cardType != ShopCardType.NewCard)
        {
            NewCardsPriority += OtherGroupsPriorityBonus;
        } else
        {
            NewCardsPriority = 0;
        }
        if (cardType != ShopCardType.MaxHealth)
        {
            MaxHealthPriority += OtherGroupsPriorityBonus;
        } else
        {
            MaxHealthPriority = 0;
        }
        if (cardType != ShopCardType.BaseDefence)
        {
            BaseDefencePriority += OtherGroupsPriorityBonus;
        } else
        {
            BaseDefencePriority = 0;
        }

        Debug.Log(
            $"[Shop] Priorities: Potions={PotionsPriority} Dice={DicePriority} Roll Size={RollSizePriority} Hand Size={HandSizePriority} Destroy Card={DestructablePriority} New Card={NewCardsPriority}"
            + $" Max Health={MaxHealthPriority} Base Defence={BaseDefencePriority}"
            );
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
            case ShopCardType.NewCard:
                var newSetting = NewCards[0];
                NewCards.RemoveAt(0);
                card.Prepare(
                    newSetting.Name,
                    newSetting.Summary(),
                    newSetting.Sprite,
                    newSetting.ShopCost,
                    delegate
                    {
                        ShopDeck.instance.RemoveOneInstance(newSetting);
                        ActionDeck.instance.AddCard(newSetting);
                        GameProgress.XP -= newSetting.ShopCost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            case ShopCardType.MaxHealth:
                var maxHealthCost = increaseMaxHealth.Cost(GameProgress.IncreasedMaxHealth + 1);
                card.Prepare(
                    increaseMaxHealth.Name,
                    "Increase player max health",
                    increaseMaxHealth.Sprite,
                    maxHealthCost,
                    delegate
                    {
                        GameProgress.IncreaseMaxHealth();
                        GameProgress.XP -= maxHealthCost;
                        ValidateCardCosts();
                        playerStats.UpdateStats();
                    }
                );
                break;
            case ShopCardType.BaseDefence:
                var baseDefenceCost = increaseBaseDefence.Cost(GameProgress.BaseDefence + 1);
                card.Prepare(
                    increaseBaseDefence.Name,
                    "Increase player base defence",
                    increaseBaseDefence.Sprite,
                    baseDefenceCost,
                    delegate
                    {
                        GameProgress.IncreaseBaseDefence();
                        GameProgress.XP -= baseDefenceCost;
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
        bool noHealthInShop = true;
        for (int i = 0; i < GameSettings.MaxShowWares; i++)
        {
            var cardType = GetCardType(noHealthInShop);
            if (cardType == ShopCardType.Potion)
            {
                noHealthInShop = false;
            }
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
