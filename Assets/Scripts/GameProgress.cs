using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgress : DeCrawl.Primitives.FindingSingleton<GameProgress> 
{
    #region Card Hand Size
    int StartCardHandSize = 2;
    int _cardHandSize = -1;
    int cardHandSize
    {
        get
        {
            if (_cardHandSize < 0)
            {
                _cardHandSize = StartCardHandSize;
            }
            return _cardHandSize;
        }

        set
        {
            _cardHandSize = Mathf.Clamp(value, StartCardHandSize, GameSettings.MaxPlayerCardHandSize);
        }
    }
    public static int CardHandSize
    {
        get => instance.cardHandSize;
    }
    public static void IncreaseCardHandSize()
    {
        instance.cardHandSize++;
    }
    #endregion

    #region Max Health
    [SerializeField, Tooltip("If negative, same as start health")]
    int StartMaxHealth = -1;
    int _maxHealth = -1;
    int maxHealth
    {
        get
        {
            if (_maxHealth < 0)
            {
                if (StartMaxHealth < 0)
                {
                    _maxHealth = StartHealth;
                } else
                {
                    _maxHealth = StartMaxHealth;
                }
            }
            return _maxHealth;
        }
        set
        {
            _maxHealth = Mathf.Max(0, value);
            if (health > _maxHealth)
            {
                health = _maxHealth;
            }
        }
    }
    public static int MaxHealth
    {
        get => instance.maxHealth;
        set
        {
            instance.maxHealth = value;
        }
    }
    #endregion

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
            _health = Mathf.Clamp(value, 0, maxHealth);
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

    #region Dice Roll Size
    [SerializeField]
    int StartRollSize = 3;
    int _rollSize = -1;
    int rollSize
    {
        get
        {
            if (_rollSize < 0)
            {
                _rollSize = StartRollSize;
            }
            return _rollSize;
        }

        set
        {
            _rollSize = Mathf.Clamp(value, 0, GameSettings.MaxPlayerRollSize);
        }
    }
    public static int RollSize
    {
        get => instance.rollSize;
    }
    public static bool CanIncreaseDiceHand => instance.rollSize < GameSettings.MaxPlayerRollSize;
    public static void IncreaseRollSize()
    {
        instance.rollSize++;
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

    #region Scenes
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
    private static string _gameOverScene = "GameOver";
    public static void InvokeGameOver()
    {
        SceneManager.LoadScene(_gameOverScene);
    }
    #endregion

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
