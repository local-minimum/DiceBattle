using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgress : DeCrawl.Primitives.FindingSingleton<GameProgress> 
{
    #region Health
    [SerializeField]
    int StartHealth = 42;
    int _health = -1;
    int health
    {
        get { 
            if (_health < 0)
            {
                _health = StartHealth;
            }
            return _health;
        }
        set
        {
            _health = Mathf.Max(0, value);
        }
    }
    public static int Health
    {
        get => instance.health;
        set
        {
            instance.health = value;
        }
    }
    #endregion

    #region Dice
    [SerializeField]
    int StartDice = 20;
    int _dice = -1;
    int dice
    {
        get
        {
            if (_dice < 0)
            {
                _dice = StartDice;
            }
            return _dice;
        }
        set
        {
            _dice = Mathf.Max(0, value);
        }
    }
    public static int Dice
    {
        get => instance.dice;
        set
        {
            instance.dice = value;
        }
    }
    #endregion

    #region Dice Hand Size
    [SerializeField]
    int StartDiceHandSize = 3;
    int _diceHandSize = -1;
    int diceHandSize
    {
        get
        {
            if (_diceHandSize < 0)
            {
                _diceHandSize = StartDiceHandSize;
            }
            return _diceHandSize;
        }

        set
        {
            _diceHandSize = Mathf.Clamp(value, 0, GameSettings.MaxDiceHand);
        }
    }
    public static int DiceHandSize
    {
        get => instance.diceHandSize;
    }
    public static bool CanIncreaseDiceHand => instance.diceHandSize < GameSettings.MaxDiceHand;
    public static void IncreaseDiceHand()
    {
        instance.diceHandSize++;
    }
    #endregion

    #region XP
    int _xp = 0;
    public int xp
    {
        get => _xp;
        set
        {
            _xp = Mathf.Max(0, value);
        }
    }
    public static int XP
    {
        get => instance.xp;
        set
        {
            instance.xp = value;
        }
    }
    #endregion

    #region Fights
    int _fights = 0;
    public static int Fights
    {
        get => instance._fights;
    }
    public static void IncreaseFights()
    {
        instance._fights++;
    }
    #endregion

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private static string _nextScene = "XP-Shop";
    public static void NextScene()
    {
        Debug.Log($"Next scene is {_nextScene}");
        SceneManager.LoadScene(_nextScene);
    }
    private static string _battleScene = "Battle";
    public static void InvokeBattle()
    {
        SceneManager.LoadScene(_battleScene);
    }
}
