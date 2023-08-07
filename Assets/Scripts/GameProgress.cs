using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
